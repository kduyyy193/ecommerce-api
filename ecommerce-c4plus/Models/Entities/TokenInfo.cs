using System;
using System.ComponentModel.DataAnnotations;

namespace ecommerce_c4plus.Models
{
    public class TokenInfoDTO
    {
        public int userId { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string refreshToken { get; set; }
        public DateTime accessTokenExpiration { get; set; }
        public DateTime refreshTokenExpiration { get; set; }

        public TokenInfoDTO()
        {
            AccessToken = "";
            refreshToken = "";
            accessTokenExpiration = DateTime.MinValue;
            refreshTokenExpiration = DateTime.MinValue;
        }
    }
}