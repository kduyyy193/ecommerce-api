using ecommerce_c4plus.Models.DTOs.Request;
using ecommerce_c4plus.Models.DTOs.Response;
using ecommerce_c4plus.Repository;

namespace ecommerce_c4plus.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepository;

        public CustomerService(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<APIResponse<TokenResponse>> RegisterCustomer(SignUpRequest userRegister)
        {
            try
            {
                var result = await _customerRepository.Register(userRegister);
                return result;
            }
            catch (Exception ex)
            {
                return new APIResponse<TokenResponse>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
