using System.Security.Claims;
using TicketBookingService.Common;

namespace TicketBookingService.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int UserId { get; }
        public string Role { get; }

        public string Email { get; }
        public string FullName { get; }

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            var userIdClaim = user?.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out int uid))
                UserId = uid;
            Email = user?.FindFirst(ClaimTypes.Email)?.Value?? string.Empty;
            FullName = user?.FindFirst("name")?.Value?? string.Empty;


            Role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
