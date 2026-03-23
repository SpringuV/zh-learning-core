
using HanziAnhVu.Shared.Domain.Exceptions;

namespace HanziAnhVuHsk.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string message = "An internal server error occurred.";

            // Thay vì if/else từng loại AuthDomainException, PaymentDomainException...
            // Chúng ta chỉ cần gom tất cả các lỗi có đặc điểm "DomainException"
            if (exception is DomainException domainEx)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = domainEx.Message;
            }
            // Thêm các loại exception dùng chung khác, ví dụ: NotFoundException, ValidationException...

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var result = new
            {
                error = message
                // có thể thêm stack trace cho môi trường development
            };

            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
