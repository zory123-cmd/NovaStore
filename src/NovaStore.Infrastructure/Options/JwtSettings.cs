namespace NovaStore.Infrastructure.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    // JWT_KEY is read exclusively from the JWT_KEY environment variable in Program.cs
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 1440;
}
