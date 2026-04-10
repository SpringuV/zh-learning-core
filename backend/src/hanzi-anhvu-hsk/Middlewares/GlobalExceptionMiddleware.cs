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
            ErrorCode errorCode = ErrorCode.INTERNAL_ERROR;
            string message = "An internal server error occurred.";
            int statusCode = StatusCodes.Status500InternalServerError;

            if (exception is DomainException domainEx)
            {
                errorCode = ErrorCode.VALIDATION;
                message = domainEx.Message;
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is KeyNotFoundException notFoundEx)
            {
                errorCode = ErrorCode.NOTFOUND;
                message = notFoundEx.Message;
                statusCode = StatusCodes.Status404NotFound;
            }
            else if (exception is ArgumentException argEx)
            {
                errorCode = ErrorCode.VALIDATION;
                message = argEx.Message;
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is InvalidOperationException opEx)
            {
                errorCode = ErrorCode.INVALID_STATE;
                message = opEx.Message;
                statusCode = StatusCodes.Status400BadRequest;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var result = new Result
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode.ToString()
            };

            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
