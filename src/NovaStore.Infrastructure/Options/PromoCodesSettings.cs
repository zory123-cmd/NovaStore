namespace NovaStore.Infrastructure.Options;

public class PromoCodesSettings
{
    public const string SectionName = "PromoCodes";
    public Dictionary<string, PromoCodeConfig> Codes { get; set; } = new();
}

public class PromoCodeConfig
{
    public string Type { get; set; } = "FIXED";
    public decimal Value { get; set; }
}
