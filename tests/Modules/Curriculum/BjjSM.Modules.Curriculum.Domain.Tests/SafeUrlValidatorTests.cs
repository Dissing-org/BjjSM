using BjjSM.SharedKernel;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class SafeUrlValidatorTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://example.com/drill")]
    [InlineData("  https://bjj.example.org/video?id=1  ")]
    public void IsSafeExternalUrl_ValidHttpOrHttps_ReturnsTrue(string url)
    {
        Assert.True(SafeUrlValidator.IsSafeExternalUrl(url));
        Assert.Equal(url.Trim(), SafeUrlValidator.SanitizeExternalUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("javascript:alert(1)")]
    [InlineData("JAVASCRIPT:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("file:///etc/passwd")]
    [InlineData("//evil.example.com/payload")]
    [InlineData("/media/techniques/demo.mp4")]
    [InlineData("images/techniques/demo.gif")]
    public void IsSafeExternalUrl_UnsafeOrRelative_ReturnsFalse(string? url)
    {
        Assert.False(SafeUrlValidator.IsSafeExternalUrl(url));
        Assert.Null(SafeUrlValidator.SanitizeExternalUrl(url));
    }

    [Theory]
    [InlineData("https://cdn.example.com/video.mp4")]
    [InlineData("http://cdn.example.com/photo.jpg")]
    [InlineData("/media/techniques/20261018_dlr_guard.mp4")]
    [InlineData("images/techniques/dlr-single-leg-x.gif")]
    public void IsSafeMediaUrl_ValidHttpHttpsOrRelativeMedia_ReturnsTrue(string url)
    {
        Assert.True(SafeUrlValidator.IsSafeMediaUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/svg+xml,<svg onload=alert(1)>")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("//evil.example.com/video.mp4")]
    [InlineData("/media/../secrets/config.json")]
    [InlineData("images/../../etc/passwd")]
    [InlineData("/media/techniques\\evil.mp4")]
    [InlineData("/other/path.mp4")]
    public void IsSafeMediaUrl_UnsafeOrTraversal_ReturnsFalse(string? url)
    {
        Assert.False(SafeUrlValidator.IsSafeMediaUrl(url));
    }
}
