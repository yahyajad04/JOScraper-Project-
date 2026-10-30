using HtmlAgilityPack;
using JoSale.DTO_s.ItemsDTO_s;
using JoSale.Helpers;
using JoSale.Interfaces;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using JoSale.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace JoSale.Services
{
    public class CityCenterScraper : IStoreScraper<CityCenterScraper>
    {
        private readonly HttpClient _httpClient;
        private readonly IItemRepository _itemRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreItem _storeItemRepository;
        private readonly ICatergorization _categorization;
        public CityCenterScraper(HttpClient httpClient, IItemRepository itemRepository,
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
            var Store = await _storeRepository.GetByName("CityCenter");
            var url = "https://www.citycenter.jo/";

            if (Store == null)
            {
                Store = new Store
                {
                    Name = "CityCenter",
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
            var finalScrapedItems = new List<ScrapedItem>();

            //This is the Category Link That we want to crawl into
            var linksBefore = document.DocumentNode
                .SelectNodes("//*[contains(@class, 'panel-body')]//a");

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
                        var html3 = await _httpClient.GetStringAsync(fullLink2);
                        document3.LoadHtml(html3);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }

                    var productsBefore = document3.DocumentNode
                    .SelectNodes("//*[contains(@class, 'product-thumb')]");

                    if (productsBefore == null)
                    {
                        node = document3.DocumentNode
                            .SelectSingleNode("//ul[contains(@class, 'links')]/li[contains(@class, 'next')]//a")?
                            .GetAttributeValue("href", "") ?? null;

                        continue;
                    }

                    //This is the entire product element we got from the current Category and current Page
                    var products = productsBefore.ToList();

                    //This loops Extract the product info such as the name/price/etc.. to create the new Items
                    foreach (var product in products)
                    {
                        var scraped = await ExtractItem(product);
                        var newItem = scraped.Item;

                        var check = currentStoreItems.FirstOrDefault(s => s.ProductUrl.Equals(scraped.ProductURL)
                        || s.Item.NormalizedName == scraped.Item.NormalizedName);
                        if (check != null)
                        {
                            if(check.StoreId != store.Id)
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
                            newItem.Category = category.Category;

                            finalScrapedItems.Add(scraped);
                            finalItems.Add(newItem);
                        }
                    }

                    //This is to check if the pagination ended or not, if not it will bring the next page and 
                    //the loop continues, if there is no next page then the loop will end
                    //and we will go to next category
                    node = document3.DocumentNode
                    .SelectSingleNode("//ul[contains(@class, 'links')]/li[contains(@class, 'next')]//a")?
                    .GetAttributeValue("href", "") ?? null;
                }
            }

            var newItems = await _itemRepository.CreateItems(finalItems);
            var removeList = new List<ScrapedItem>();
            foreach (var scraped in finalScrapedItems)
            {
                if (newItems.ContainsKey(scraped.Item))
                    scraped.Item = newItems.GetValueOrDefault(scraped.Item);
                else
                    removeList.Add(scraped);
            }

            finalScrapedItems.RemoveAll(sc => removeList.Contains(sc));

            return finalScrapedItems;
        }

        public async Task<ScrapedItem> ExtractItem(HtmlNode product)
        {
            var name = product
                           .SelectSingleNode(".//*[contains(@class , 'caption')]//h4//a")?.InnerHtml ?? "";
            var normalizedName = NormalizeName(name);

            var description = product
                .SelectSingleNode(".//*[contains(@class , 'caption')]//div")?.InnerHtml ?? "";

            var price = product
                .SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' tb_integer ')]")
                ?.InnerHtml.Trim() ?? "0";

            var dec = product
                .SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' tb_decimal ')]")
                ?.InnerHtml.Trim() ?? "00";

            var finalPrice = decimal.Parse(price + "." + dec);

            var productLink = product
                .SelectSingleNode(".//*[contains(@class , 'caption')]//a")?
                .GetAttributeValue("href", "");

            var imageURL = product
                .SelectSingleNode(".//*[contains(@class , 'image')]//img")?
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
