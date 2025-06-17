namespace TicketBookingService.Common
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Role { get; }
        public string Email { get; }
        public string FullName { get; }
    }
}
