using HMS_360_PMS.HMS_360_PMS.API.Models;
using System.Net;
using System.Text.Json;

namespace HMS_360_PMS.HMS_360_PMS.API.MIddlewares
{
    //    public class GlobalExceptionMiddleware
    //    {
    //        private readonly RequestDelegate _next;
    //        private readonly ILogger<GlobalExceptionMiddleware> _logger;

    //        public GlobalExceptionMiddleware(RequestDelegate next,
    //            ILogger<GlobalExceptionMiddleware> logger)
    //        {
    //            _next = next;
    //            _logger = logger;
    //        }

    //        public async Task Invoke(HttpContext context)
    //        {
    //            if (context.Request.Path.StartsWithSegments("/swagger"))
    //            {
    //                await _next(context);
    //                return;
    //            }

    //            try
    //            {
    //                await _next(context);
    //            }
    //            catch (UnauthorizedAccessException ex)
    //            {
    //                _logger.LogWarning(ex, "Unauthorized access attempt");
    //                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    //                //await context.Response.WriteAsync(JsonSerializer.Serialize(
    //                //    ApiResponse.Fail("Unauthorized")));
    //    //            await context.Response.WriteAsync(JsonSerializer.Serialize(
    //    //ApiResponse<object>.Fail("Unauthorized")));

    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.LogError(ex, ex.Message);
    //                context.Response.StatusCode = 500;
    //                //await context.Response.WriteAsync(JsonSerializer.Serialize(
    //                //    ApiResponse.Fail("Internal server error")));
    //                await context.Response.WriteAsync(JsonSerializer.Serialize(
    //    ApiResponse<object>.Fail("Internal server error")));
    //            }
    //        }
    //    }

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Skip Swagger
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access");

                await HandleExceptionAsync(context,
                    HttpStatusCode.Unauthorized,
                    "Unauthorized access");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await HandleExceptionAsync(context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await HandleExceptionAsync(context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, ex.Message);

                //await HandleExceptionAsync(context,
                //    HttpStatusCode.InternalServerError,
                //    "Internal server error");

                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = ex.Message,   // 🔥 show real error
                    detail = ex.InnerException?.Message
                }));
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure(message);

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }

}
