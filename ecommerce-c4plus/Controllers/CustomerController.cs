using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ecommerce_c4plus.IRepository;
using ecommerce_c4plus.Models.DTOs.Request;
using ecommerce_c4plus.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ecommerce_c4plus.Controllers
{
    [ApiController]
    [Route("api /[controller]")]
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest newUser)
        {
            // Your sign up logic here
            var user = await _customerRepository.Register(newUser);
             
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("Failed to create user");
            }
        }
            
        [HttpPost("login")]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Login([FromBody] LoginRequest user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var result = await _customerRepository.Login(user);

            return result != null ? Ok(result) : NotFound();
        }
    }
}
