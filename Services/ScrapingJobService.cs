using JoSale.Interfaces;

namespace JoSale.Services
{
    public class ScrapingJobService : IScrapingJob
    {
        private readonly IStoreScraper<MidasStore> _midas;
        private readonly IStoreScraper<OrientalStoreScraper> _oriental;
        private readonly IStoreScraper<CityCenterScraper> _cityCenter;

        public ScrapingJobService(IStoreScraper<MidasStore> midas, IStoreScraper<OrientalStoreScraper> oriental,
            IStoreScraper<CityCenterScraper> cityCenter)
        {
            _midas = midas;
            _oriental = oriental;
            _cityCenter = cityCenter;
        }
        public async Task RunCityCenter()
        {
            await _cityCenter.ScrapeStore();
        }

        public async Task RunMidas()
        {
            await _midas.ScrapeStore();
        }

        public async Task RunOriental()
        {
             await _oriental.ScrapeStore();
        }
    }
}
