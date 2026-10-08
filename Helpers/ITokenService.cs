using JoSale.Models;

namespace JoSale.Helpers
{
    public interface ITokenService
    {
        Task<string> GetToken(AppUser user);
    }
}
