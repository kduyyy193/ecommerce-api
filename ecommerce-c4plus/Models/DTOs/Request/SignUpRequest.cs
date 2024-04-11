using System;
namespace ecommerce_c4plus.Models.DTOs.Request
{
	public class SignUpRequest
	{
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}