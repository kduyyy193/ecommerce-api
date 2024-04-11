using System;
namespace ecommerce_c4plus.Models.DTOs.Response
{
    public class APIResponse<T>
    {
        public int ResponseCode { get; set; }
        public T? Result { get; set; }
        public string Message { get; set; } = "Success";
    }

    public class APIResponse
    {
        public int ResponseCode { get; set; }
        public string Message { get; set; } = "Success";
    }
}

