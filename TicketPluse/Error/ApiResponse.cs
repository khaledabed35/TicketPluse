using System;

namespace TicketPluse.Error
{
    public class ApiResponse
    {
        public int Status_code { get; set; }
        public string? Message { get; set; }

        public ApiResponse(int status, string? message = null)
        {
            Status_code = status;

            Message = message ?? GetDefaultMessage(status);
        }

        private string? GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => null
            };
        }
    }
}