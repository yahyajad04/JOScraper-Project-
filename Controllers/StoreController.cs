using JoSale.DTO_s.StoreDTO_s;
using JoSale.Interfaces.Repositories;
using JoSale.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JoSale.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ILogger<StoreController> _logger;
        public StoreController(IStoreRepository storeRepository, ILogger<StoreController> logger)
        {
            _storeRepository = storeRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Get All Stores Initiated");

                var stores = await _storeRepository.GetAll();
                var storesDTO = stores.Select(s => s.ToStoreDTO());

                elapsed.Stop();
                _logger.LogInformation("Get All Stores Successful, Elapsed Time: {Time}ms"
                    , elapsed.Elapsed.TotalMilliseconds);

                return Ok(storesDTO);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Get All Stores Failed");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{StoreId}")]
        public async Task<IActionResult> GetById([FromRoute] int StoreId)
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Get Store By Id Initiated");

                var store = await _storeRepository.GetById(StoreId);

                elapsed.Stop();
                _logger.LogInformation("Get Store By Id Successful, Elapsed Time: {Time}ms"
                    , elapsed.Elapsed.TotalMilliseconds);

                return Ok(store.ToStoreDTO());
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Get Store By Id Failed");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreDTO createStoreDTO)
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Create {Name} Store Initiated", createStoreDTO.Name);

                var store = createStoreDTO.FromCreateDTO();
                await _storeRepository.CreateStore(store);

                elapsed.Stop();
                _logger.LogInformation("Create {Name} Store Successful,  Elapsed Time: {Time}ms"
                    , createStoreDTO.Name, elapsed.Elapsed.TotalMilliseconds);

                return Ok("Created Successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Create {Name} Store Failed"
                    , createStoreDTO.Name);
                return BadRequest(ex.Message);
            }
        }
    }
}
