namespace JoSale.Interfaces
{
    public interface IScrapingJob
    {
        Task RunMidas();
        Task RunOriental();
        Task RunCityCenter();
    }
}
