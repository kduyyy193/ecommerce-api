using System;
namespace ecommerce_c4plus.Models.DTOs.Response
{
	public class TokenResponse
	{
        public string? accessToken { get; set; }
        public string? refreshToken { get; set; }
	}
}

