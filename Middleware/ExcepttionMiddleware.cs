using System.Net;
using System.Text.Json;
using API.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace API.Middleware
{
    public class ExcepttionMiddleware
    {
        public RequestDelegate _next { get; }
        public ILogger<ExceptionHandlerMiddleware> _logger { get; }
        public IHostEnvironment _env { get; }
        public ExcepttionMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex){
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace ?.ToString())
                : new ApiException(context.Response.StatusCode, ex.Message, "Error interno del servidor");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}