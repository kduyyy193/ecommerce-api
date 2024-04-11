using System;
namespace ecommerce_c4plus.Models
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

