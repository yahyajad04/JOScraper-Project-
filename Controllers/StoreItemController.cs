using JoSale.Helpers;
using JoSale.Interfaces;
using JoSale.Interfaces.IServices;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using JoSale.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JoSale.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreItemController : ControllerBase
    {
        private readonly IStoreScraper<OrientalStoreScraper> _orientalScraper;
        private readonly IStoreScraper<CityCenterScraper> _cityCenterScraper;
        private readonly IStoreScraper<MidasStore> _midasStoreScraper;
        private readonly IStoreItem _storeItemRepository;
        private readonly ILogger<StoreItemController> _logger;
        private readonly ICacheService _cache;
        public StoreItemController(IStoreScraper<OrientalStoreScraper> orientalScraper, IStoreItem storeItem,
            IStoreScraper<CityCenterScraper> cityCenterScraper, IStoreScraper<MidasStore> midasStoreScraper,
            ILogger<StoreItemController> logger, ICacheService cache) 
        {
            _orientalScraper = orientalScraper;
            _storeItemRepository = storeItem;
            _cityCenterScraper = cityCenterScraper;
            _midasStoreScraper = midasStoreScraper;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("{StoreId}")]
        public async Task<IActionResult> GetAllFromStore([FromRoute] int StoreId, [FromQuery] QueryObject query)
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Get All From Store with Id: {Id} Initiated", StoreId);

                var key = $"store-items-{StoreId}";
                var storeItems = await _cache.GetAsync<List<StoreItem>>(key);

                //If it is a cache hit it will return
                if(storeItems != null) {
                    elapsed.Stop();
                    _logger.LogInformation("Get All From Store with Id : {Id} Successful, Elapsed Time: {Time}ms"
                        , StoreId, elapsed.Elapsed.TotalMilliseconds);
                    return Ok(storeItems);
                }

                //If it is a Cache miss will come here
                var databaseStoreItems = await _storeItemRepository.GetAllFromStore(StoreId);
                await _cache.SetAsync(key, databaseStoreItems, TimeSpan.FromHours(12));

                elapsed.Stop();
                _logger.LogInformation("Get All From Store with Id : {Id} Successful, Elapsed Time: {Time}ms"
                    ,StoreId, elapsed.Elapsed.TotalMilliseconds);
                return Ok(databaseStoreItems);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Get All From Store with Id: {Id} Failed"
                    , StoreId);
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetByItem/{ItemId}")]
        public async Task<IActionResult> GetAllByItemId([FromRoute] int ItemId)
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Get All By Item Id: {Id} Initiated", ItemId);

                var Items = await _storeItemRepository.GetAllByItemId(ItemId);

                elapsed.Stop();
                _logger.LogInformation("Get All By Item Id: {Id} Successful, Elapsed Time: {Time}ms"
                    ,ItemId, elapsed.Elapsed.TotalMilliseconds);
                return Ok(Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Get All By Item Id: {Id} Failed"
                    ,ItemId);
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("OrientalStore")]
        public async Task<IActionResult> RefreshOrientalStore()
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Scraping Oriental Store Initiated");

                var refresh = await _orientalScraper.ScrapeStore();

                elapsed.Stop();
                _logger.LogInformation("Scraping Oriental Store Successful, Elapsed Time: {Time}ms"
                    ,elapsed.Elapsed.TotalMilliseconds);

                return Ok("Refreshed Successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Scraping Oriental Store Failed");
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("CityCenter")]
        public async Task<IActionResult> RefreshCityCenter()
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Scraping CityCenter Store Initiated");

                var refresh = await _cityCenterScraper.ScrapeStore();

                elapsed.Stop();
                _logger.LogInformation("Scraping CityCenter Store Successful, Elapsed Time: {Time}ms"
                    , elapsed.Elapsed.TotalMilliseconds);
                return Ok("Refreshed Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scraping CityCenter Store Failed");
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("MidasStore")]
        public async Task<IActionResult> RefreshMidasStore()
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Scraping Midas Store Initiated");

                var refresh = await _midasStoreScraper.ScrapeStore();

                elapsed.Stop();
                _logger.LogInformation("Scraping Midas Store Successful, Elapsed Time: {Time}ms"
                    ,elapsed.Elapsed.TotalMilliseconds);
                return Ok("Refreshed Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scraping Midas Store Failed");
                return BadRequest(ex.Message);
            }

        }
    }
}
