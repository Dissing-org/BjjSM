using System.Text.RegularExpressions;

namespace BjjSM.Modules.Curriculum.Domain;

public enum MediaProvider
{
    YouTube = 1,
    DirectVideo = 2,
    LocalServerVideo = 3,
    ImageOrGif = 4
}

public sealed record ParsedMediaInfo(
    string RawUrl,
    string CleanUrl,
    MediaProvider Provider,
    int? StartSeconds,
    int? EndSeconds,
    string? EmbedUrl);

public static class MediaUrlParser
{
    private static readonly Regex YouTubeWatchRegex = new(
        @"^(?:https?:\/\/)?(?:www\.)?youtube\.com\/watch\?(?:.*&)?v=([a-zA-Z0-9_-]{11})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YouTubeShortUrlRegex = new(
        @"^(?:https?:\/\/)?youtu\.be\/([a-zA-Z0-9_-]{11})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YouTubeShortsRegex = new(
        @"^(?:https?:\/\/)?(?:www\.)?youtube\.com\/shorts\/([a-zA-Z0-9_-]{11})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YouTubeEmbedRegex = new(
        @"^(?:https?:\/\/)?(?:www\.)?youtube\.com\/embed\/([a-zA-Z0-9_-]{11})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".ogg", ".mov", ".m4v"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg", ".bmp"
    };

    public static ParsedMediaInfo Parse(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return new ParsedMediaInfo(string.Empty, string.Empty, MediaProvider.ImageOrGif, null, null, null);
        }

        var trimmed = rawUrl.Trim();
        var (cleanUrl, startSec, endSec) = ExtractCleanUrlAndTimestamps(trimmed);

        // 1. YouTube Detection
        var ytMatch = YouTubeWatchRegex.Match(trimmed);
        if (!ytMatch.Success) ytMatch = YouTubeShortUrlRegex.Match(trimmed);
        if (!ytMatch.Success) ytMatch = YouTubeShortsRegex.Match(trimmed);
        if (!ytMatch.Success) ytMatch = YouTubeEmbedRegex.Match(trimmed);

        if (ytMatch.Success)
        {
            var videoId = ytMatch.Groups[1].Value;
            var embedUrl = BuildYouTubeEmbedUrl(videoId, startSec, endSec);
            return new ParsedMediaInfo(trimmed, cleanUrl, MediaProvider.YouTube, startSec, endSec, embedUrl);
        }

        // 2. Local Server Video
        var cleanPathOnly = cleanUrl.Split('?', '#')[0];
        var extension = Path.GetExtension(cleanPathOnly);

        var isLocal = cleanUrl.StartsWith("/media/", StringComparison.OrdinalIgnoreCase)
                      || cleanUrl.StartsWith("media/", StringComparison.OrdinalIgnoreCase);

        if (isLocal && VideoExtensions.Contains(extension))
        {
            var embedUrl = BuildMediaFragmentUrl(cleanUrl, startSec, endSec);
            return new ParsedMediaInfo(trimmed, cleanUrl, MediaProvider.LocalServerVideo, startSec, endSec, embedUrl);
        }

        // 3. Direct Video (.mp4, .webm, etc.)
        if (VideoExtensions.Contains(extension))
        {
            var embedUrl = BuildMediaFragmentUrl(cleanUrl, startSec, endSec);
            return new ParsedMediaInfo(trimmed, cleanUrl, isLocal ? MediaProvider.LocalServerVideo : MediaProvider.DirectVideo, startSec, endSec, embedUrl);
        }

        // 4. Default to Image or GIF
        return new ParsedMediaInfo(trimmed, cleanUrl, MediaProvider.ImageOrGif, null, null, cleanUrl);
    }

    private static (string cleanUrl, int? start, int? end) ExtractCleanUrlAndTimestamps(string raw)
    {
        int? start = null;
        int? end = null;

        var urlWithoutFragment = raw;
        var hashIdx = raw.IndexOf('#');
        if (hashIdx >= 0)
        {
            var fragment = raw.Substring(hashIdx + 1);
            urlWithoutFragment = raw.Substring(0, hashIdx);

            if (fragment.StartsWith("t=", StringComparison.OrdinalIgnoreCase))
            {
                var timePart = fragment.Substring(2);
                var parts = timePart.Split(',');
                if (parts.Length > 0 && TryParseSeconds(parts[0], out var s))
                {
                    start = s;
                }
                if (parts.Length > 1 && TryParseSeconds(parts[1], out var e))
                {
                    end = e;
                }
            }
        }

        // Check query parameters for t=, start=, end=
        var queryIdx = urlWithoutFragment.IndexOf('?');
        if (queryIdx >= 0)
        {
            var queryString = urlWithoutFragment.Substring(queryIdx + 1);
            var queryParams = queryString.Split(new[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var q in queryParams)
            {
                var kvp = q.Split('=', 2);
                if (kvp.Length != 2) continue;
                var key = kvp[0].Trim();
                var val = kvp[1].Trim();

                if (key.Equals("start", StringComparison.OrdinalIgnoreCase) && int.TryParse(val, out var startVal))
                {
                    start ??= startVal;
                }
                else if (key.Equals("end", StringComparison.OrdinalIgnoreCase) && int.TryParse(val, out var endVal))
                {
                    end ??= endVal;
                }
                else if (key.Equals("t", StringComparison.OrdinalIgnoreCase) && TryParseSeconds(val, out var tVal))
                {
                    start ??= tVal;
                }
            }
        }

        // Clean URL keeps scheme/host/path and strips timestamp query/fragments
        var clean = urlWithoutFragment;
        if (clean.Contains('?'))
        {
            var basePart = clean.Substring(0, clean.IndexOf('?'));
            var qs = clean.Substring(clean.IndexOf('?') + 1);
            var preserved = qs.Split(new[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !p.StartsWith("t=", StringComparison.OrdinalIgnoreCase)
                         && !p.StartsWith("start=", StringComparison.OrdinalIgnoreCase)
                         && !p.StartsWith("end=", StringComparison.OrdinalIgnoreCase))
                .ToList();

            clean = preserved.Count > 0 ? $"{basePart}?{string.Join("&", preserved)}" : basePart;
        }

        return (clean, start, end);
    }

    private static bool TryParseSeconds(string input, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim().ToLowerInvariant().TrimEnd('s');

        if (int.TryParse(input, out seconds))
        {
            return true;
        }

        var match = Regex.Match(input, @"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+))?$");
        if (match.Success)
        {
            var h = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
            var m = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
            var s = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            seconds = (h * 3600) + (m * 60) + s;
            return true;
        }

        return false;
    }

    public static string BuildUrlWithTimestamps(string baseUrl, int? startSeconds, int? endSeconds)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return string.Empty;
        var clean = baseUrl.Trim().Split('#')[0];
        if (startSeconds.HasValue && endSeconds.HasValue)
        {
            return $"{clean}#t={startSeconds.Value},{endSeconds.Value}";
        }
        if (startSeconds.HasValue)
        {
            return $"{clean}#t={startSeconds.Value}";
        }
        return clean;
    }

    private static string BuildYouTubeEmbedUrl(string videoId, int? start, int? end)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"https://www.youtube.com/embed/{videoId}?autoplay=1&mute=1&loop=1&playlist={videoId}&controls=1");
        if (start.HasValue) sb.Append($"&start={start.Value}");
        if (end.HasValue) sb.Append($"&end={end.Value}");
        return sb.ToString();
    }

    private static string BuildMediaFragmentUrl(string cleanUrl, int? start, int? end)
    {
        if (start.HasValue && end.HasValue)
        {
            return $"{cleanUrl}#t={start.Value},{end.Value}";
        }
        if (start.HasValue)
        {
            return $"{cleanUrl}#t={start.Value}";
        }
        return cleanUrl;
    }
}
