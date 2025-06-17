using AuthService.Common;

namespace AuthService.Models
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AddOrUpdateUserRequest
    {
        public int? Id { get; set; } // Null for Add, non-null for Update
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public UserRolesEnum Role { get; set; }
    }
}
