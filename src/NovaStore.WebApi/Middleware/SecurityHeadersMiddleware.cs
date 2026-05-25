namespace NovaStore.WebApi.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var nonce = Guid.NewGuid().ToString("N");
            context.Items["CSP-Nonce"] = nonce;

            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

            var csp = $"default-src 'self'; script-src 'self'; style-src 'self' 'nonce-{nonce}'; img-src 'self' data:; font-src 'self'; connect-src 'self'";
            context.Response.Headers.Append("Content-Security-Policy", csp);

            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
