namespace ApiGateway.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly Dictionary<string, RateLimitInfo> _rateLimits = new();
    private static readonly object _lock = new();

    private const int RequestsPerMinute = 100;
    private const int BurstLimit = 20;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (!IsRequestAllowed(clientId))
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded. Please try again later." });
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from JWT claims
        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private bool IsRequestAllowed(string clientId)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            if (!_rateLimits.TryGetValue(clientId, out var info))
            {
                info = new RateLimitInfo { WindowStart = now, RequestCount = 0 };
                _rateLimits[clientId] = info;
            }

            // Reset window if a minute has passed
            if ((now - info.WindowStart).TotalMinutes >= 1)
            {
                info.WindowStart = now;
                info.RequestCount = 0;
            }

            // Check burst limit (requests in last second)
            if ((now - info.LastRequest).TotalSeconds < 1 && info.BurstCount >= BurstLimit)
            {
                return false;
            }

            // Check rate limit
            if (info.RequestCount >= RequestsPerMinute)
            {
                return false;
            }

            // Update counters
            if ((now - info.LastRequest).TotalSeconds >= 1)
            {
                info.BurstCount = 0;
            }

            info.RequestCount++;
            info.BurstCount++;
            info.LastRequest = now;

            return true;
        }
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
        public int BurstCount { get; set; }
        public DateTime LastRequest { get; set; }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
