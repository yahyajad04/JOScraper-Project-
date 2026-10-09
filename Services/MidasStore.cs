using HtmlAgilityPack;
using JoSale.DTO_s.ItemsDTO_s;
using JoSale.Interfaces;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Playwright;

namespace JoSale.Services
{
    public class MidasStore : IStoreScraper<MidasStore>
    {
        private readonly HttpClient _httpClient;
        private readonly IItemRepository _itemRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreItem _storeItemRepository;
        IPage page;
        private readonly ICatergorization _categorization;
        public MidasStore(HttpClient httpClient, IItemRepository itemRepository,
            IStoreRepository storeRepository, IStoreItem storeItemRepository, ICatergorization categorization)
        {
            _httpClient = httpClient;
            _itemRepository = itemRepository;
            _storeRepository = storeRepository;
            _storeItemRepository = storeItemRepository;
            _categorization = categorization;
        }
        public async Task<List<StoreItem>> ScrapeStore()
        {
            //This is for surpassing THE Vue problem
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

            page = await browser.NewPageAsync();

            //This is the Store Check
            var Store = await _storeRepository.GetByName("MidasStore");
            var url = "https://mcc-jo.com/categories";

            if (Store == null)
            {
                Store = new Store
                {
                    Name = "MidasStore",
                    About = "This is an Electronic Store",
                    URL = url
                };

                await _storeRepository.CreateStore(Store);
            }

            var result = await CrawlStore(Store);
            var finalStoreItems = new List<StoreItem>();

            if (result.IsNullOrEmpty())
                return new List<StoreItem>();

            foreach (var scraped in result)
            {
                var storeItem = new StoreItem
                {
                    Store = Store,
                    StoreId = Store.Id,
                    Item = scraped.Item,
                    ItemId = scraped.Item.Id,
                    Price = scraped.Price,
                    LastUpdated = DateTime.UtcNow,
                    ProductUrl = scraped.ProductURL,
                    ImageURL = scraped.ImageURL
                };

                finalStoreItems.Add(storeItem);
            }
            await _storeItemRepository.CreateStoreItems(finalStoreItems);
            return finalStoreItems;
        }

        public async Task<List<ScrapedItem>> CrawlStore(Store store)
        {
            //Here we will be using HTMLAGILITYPACK, this pack gives us access to manipulate the DOM model in .NET
            //This pack queries are different than the normal DOM model that is used in javascript
            //to use it we have to know how to use the XPath
            var url = store.URL;
            var html = await _httpClient.GetStringAsync(url);

            //making the HTML to DOM
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var finalItems = new List<Item>();
            var currentStoreItems = await _storeItemRepository.GetAllFromStore(store.Id);
            var urls = currentStoreItems
                .Where(s => !string.IsNullOrEmpty(s.ProductUrl))
                .Select(s => s.ProductUrl!)
                .ToHashSet();

            var finalScrapedItems = new List<ScrapedItem>();

            //This is the Category Link That we want to crawl into
            var linksBefore = document.DocumentNode
                .SelectNodes("//*[contains(@class, 'all-categories')]//li//a");

            if (linksBefore.IsNullOrEmpty())
                return new List<ScrapedItem>();

            var links = linksBefore?.Select(a => a.GetAttributeValue("href", "")).ToList();

            if (links.IsNullOrEmpty())
                return new List<ScrapedItem>();

            //This loop is for crawling into all the Categories
            foreach (var link in links!)
            {
                var fullLink = new Uri(new Uri(url), link).ToString();

                //This the Pagination link for the Current Category We are In So we can crawl into all pages
                var node = fullLink;

                //This loop is for crawling into all the pages
                while (node != null)
                {
                    var fullLink2 = new Uri(new Uri(url), node).ToString();
                    var document3 = new HtmlDocument();
                    try
                    {
                        await page.GotoAsync(fullLink2);

                        await page.WaitForSelectorAsync(".product-card");

                        var html3 = await page.ContentAsync();

                        document3.LoadHtml(html3);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    var productsBefore = document3.DocumentNode
                        .SelectNodes("//*[contains(concat(' ', normalize-space(@class), ' '), ' product-card ')]");

                    if (productsBefore == null)
                    {
                        var temp = document3.DocumentNode
                            .SelectNodes("//ul[contains(@class, 'page-item')]/li//a");
                        if (temp != null && temp.Count > 0)
                        {
                            var lastNode = temp[temp.Count - 1];

                            var parentLi = lastNode.ParentNode;

                            if (parentLi.GetAttributeValue("class", "").Contains("disabled"))
                            {
                                node = null;
                            }
                            else
                            {
                                node = lastNode.GetAttributeValue("href", "") ?? null;
                            }
                        }
                        else
                        {
                            node = null;
                        }

                        continue;
                    }

                    //This is the entire product element we got from the current Category and current Page
                    var products = productsBefore.ToList();
                    //This loops Extract the product info such as the name/price/etc.. to create the new Items
                    foreach (var product in products)
                    {
                        var scraped = await ExtractItem(product);
                        var newItem = scraped.Item;

                        if(urls.Contains(scraped.ProductURL))
                        {
                            var check = currentStoreItems.FirstOrDefault(s => s.ProductUrl!.Equals(scraped.ProductURL)
                            || s.Item.NormalizedName == scraped.Item.NormalizedName);

                            if (check.StoreId != store.Id)
                            {
                                finalScrapedItems.Add(scraped);
                            }
                            if (check.Price != scraped.Price)
                                await _storeItemRepository.UpdatePrice(check.StoreId, check.ItemId, scraped.Price);
                        }
                        else
                        {
                            var category = await _categorization.Categorize(newItem);
                            scraped.Item.Category = category.Category;
                            scraped.Item.CategoryId = category.Category.Id;
                            scraped.Item.Vector = category.Vector;

                            newItem.Vector = category.Vector;
                            newItem.CategoryId = category.Category.Id;
                            newItem.CategoryId = category.Category.Id;

                            finalScrapedItems.Add(scraped);
                        }

                        finalItems.Add(newItem);
                    }

                    //This is to check if the pagination ended or not, if not it will bring the next page and 
                    //the loop continues, if there is no next page then the loop will end
                    //and we will go to next category
                    var nodes = document3.DocumentNode
                        .SelectNodes("//ul[contains(@class, 'page-item')]/li//a");

                    if (nodes != null && nodes.Count > 0)
                    {
                        var lastNode = nodes[nodes.Count - 1];

                        var parentLi = lastNode.ParentNode;

                        if (parentLi.GetAttributeValue("class", "").Contains("disabled"))
                        {
                            node = null;
                        }
                        else
                        {
                            node = lastNode.GetAttributeValue("href", "") ?? null;
                        }
                    }
                    else
                    {
                        node = null;
                    }
                }
            }

            var newItems = await _itemRepository.CreateItems(finalItems);
            return finalScrapedItems;
        }

        public async Task<ScrapedItem> ExtractItem(HtmlNode product)
        {
            var name = product
                           .SelectSingleNode(".//*[contains(@class , 'product-card-middle')]//a//h6//span//span")?.InnerHtml ?? "";
            var normalizedName = NormalizeName(name);

            var description = product
                .SelectSingleNode(".//*[contains(@class , 'product-card-middle')]//*[contains(@class , 'prod-desc')]")?.InnerHtml ?? "";

            var price =
                product.SelectSingleNode(".//*[contains(@class, 'product-card-bottom')]//*[contains(@class, 'special-price')]")?.InnerText.Trim()
                ??
                product.SelectSingleNode(".//*[contains(@class, 'product-card-bottom')]//*[contains(@class, 'previous-price')]")?.InnerText.Trim()
                ??
                product.SelectSingleNode(".//*[contains(@class, 'product-card-bottom')]//*[contains(@class, 'product-price')]")?.InnerText.Trim()
                ?? "0";

            var extract = price.Split(";");
            var finalPrice = decimal.Parse(extract[extract.Length-1]);

            var productLink = product
                .SelectSingleNode(".//*[contains(@class , 'product-card-middle')]//a")?
                .GetAttributeValue("href", "");

            var imageURL = product
                .SelectSingleNode(".//*[contains(@class , 'product-image')]//img")?
                .GetAttributeValue("src", "");

            var newItem = new Item
            {
                Name = name,
                NormalizedName = normalizedName,
                Description = description,
            };

            var scraped = new ScrapedItem
            {
                Item = newItem,
                Price = finalPrice,
                ProductURL = productLink,
                ImageURL = imageURL
            };
            return scraped;
        }
        public static string NormalizeName(string name)
        {
            return string.Join(" ",
                name.Trim()
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            );
        }
    }
}
