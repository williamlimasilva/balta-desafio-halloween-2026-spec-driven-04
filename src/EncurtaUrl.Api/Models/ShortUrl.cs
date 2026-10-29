namespace EncurtaUrl.Api.Models;

public class ShortUrl
{
    public int Id { get; set; }

    // Código curto usado na navegação (ex.: /aZ3k9Qx)
    public string Code { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int Clicks { get; set; }
}
