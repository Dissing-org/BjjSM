namespace BjjSM.SharedKernel;

public static class SafeUrlValidator
{
    private static readonly string[] ForbiddenSchemes =
    [
        "javascript:",
        "data:",
        "vbscript:"
    ];

    public static bool IsSafeExternalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                || uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
               && !string.IsNullOrWhiteSpace(uri.Host);
    }

    public static bool IsSafeMediaUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();

        if (IsSafeExternalUrl(trimmed))
        {
            return true;
        }

        if (trimmed.StartsWith("//", StringComparison.Ordinal)
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.Contains('\\', StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var scheme in ForbiddenSchemes)
        {
            if (trimmed.Contains(scheme, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return trimmed.StartsWith("/media/", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("images/", StringComparison.OrdinalIgnoreCase);
    }

    public static string? SanitizeExternalUrl(string? url)
    {
        if (!IsSafeExternalUrl(url))
        {
            return null;
        }

        return url!.Trim();
    }
}
