using System.Security.Claims;

namespace AuthService.Abstract
{
    public interface ITokenService
    {
        string GenerateToken(List<Claim> claims);    
    }
}
