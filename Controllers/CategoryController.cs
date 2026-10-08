using JoSale.Interfaces.Repositories;
using JoSale.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JoSale.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var elapsed = Stopwatch.StartNew();
                _logger.LogInformation("Get All Categories Initiated");

                var categories = await _categoryRepository.GetAll();
                var categoriesDTO = categories.Select(s => s.ToCategoryDTO());

                elapsed.Stop();
                _logger.LogInformation("Get All Categories Successful, Elapsed Time: {Time}ms"
                    ,elapsed.Elapsed.TotalMilliseconds);

                return Ok(categoriesDTO);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Get All Categories Failed");
                return BadRequest(ex.Message);
            }
        }
    }
}
