using System;
using ecommerce_c4plus.Models.DTOs.Request;
using ecommerce_c4plus.Models.DTOs.Response;


namespace ecommerce_c4plus.IRepository
{
	public interface ICustomerRepository
    {
        Task<APIResponse<TokenResponse>> Register(SignUpRequest user);
        Task<APIResponse<TokenResponse>> Login(LoginRequest user);
    }
}

