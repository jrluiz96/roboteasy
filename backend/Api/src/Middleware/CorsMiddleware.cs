namespace Api.Middleware;

public class CorsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _allowedOrigins;

    public CorsMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _allowedOrigins = configuration["AppSettings:CorsOrigins"] ?? "*";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        
        if (IsOriginAllowed(origin))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", _allowedOrigins == "*" ? "*" : origin);
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
            
            if (_allowedOrigins != "*")
            {
                context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            }
        }

        // Preflight request
        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 204;
            return;
        }

        await _next(context);
    }

    private bool IsOriginAllowed(string origin)
    {
        if (string.IsNullOrEmpty(origin))
            return true;

        if (_allowedOrigins == "*")
            return true;

        var allowed = _allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return allowed.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}

public static class CorsMiddlewareExtensions
{
    public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorsMiddleware>();
    }
}
