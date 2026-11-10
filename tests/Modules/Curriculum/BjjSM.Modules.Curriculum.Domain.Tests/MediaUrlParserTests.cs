using BjjSM.Modules.Curriculum.Domain;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class MediaUrlParserTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=B_dOCcWymv4#t=148,154", 148, 154)]
    [InlineData("https://www.youtube.com/watch?v=B_dOCcWymv4&start=10&end=25", 10, 25)]
    [InlineData("https://www.youtube.com/watch?v=B_dOCcWymv4?start=10&end=25", 10, 25)]
    [InlineData("https://youtu.be/B_dOCcWymv4#t=30", 30, null)]
    [InlineData("https://www.youtube.com/shorts/B_dOCcWymv4", null, null)]
    public void Parse_YouTubeUrls_ShouldDetectProviderAndBuildEmbedUrl(string url, int? expectedStart, int? expectedEnd)
    {
        var result = MediaUrlParser.Parse(url);

        Assert.Equal(MediaProvider.YouTube, result.Provider);
        Assert.Equal(expectedStart, result.StartSeconds);
        Assert.Equal(expectedEnd, result.EndSeconds);
        Assert.Contains("https://www.youtube.com/embed/B_dOCcWymv4", result.EmbedUrl);
        Assert.Contains("loop=1&playlist=B_dOCcWymv4", result.EmbedUrl);

        if (expectedStart.HasValue)
            Assert.Contains($"start={expectedStart.Value}", result.EmbedUrl);
        if (expectedEnd.HasValue)
            Assert.Contains($"end={expectedEnd.Value}", result.EmbedUrl);
    }

    [Fact]
    public void Parse_DirectVideoWithFragment_ShouldDetectDirectVideoAndAppendMediaFragment()
    {
        var url = "https://cdn.example.com/techniques/armbar.mp4#t=15,30";
        var result = MediaUrlParser.Parse(url);

        Assert.Equal(MediaProvider.DirectVideo, result.Provider);
        Assert.Equal(15, result.StartSeconds);
        Assert.Equal(30, result.EndSeconds);
        Assert.Equal("https://cdn.example.com/techniques/armbar.mp4", result.CleanUrl);
        Assert.Equal("https://cdn.example.com/techniques/armbar.mp4#t=15,30", result.EmbedUrl);
    }

    [Fact]
    public void Parse_LocalServerVideo_ShouldDetectLocalServerVideo()
    {
        var url = "/media/techniques/20261018_dlr_guard.mp4#t=5,20";
        var result = MediaUrlParser.Parse(url);

        Assert.Equal(MediaProvider.LocalServerVideo, result.Provider);
        Assert.Equal(5, result.StartSeconds);
        Assert.Equal(20, result.EndSeconds);
        Assert.Equal("/media/techniques/20261018_dlr_guard.mp4#t=5,20", result.EmbedUrl);
    }

    [Theory]
    [InlineData("images/techniques/dlr-single-leg-x.gif")]
    [InlineData("/media/techniques/diagram.png")]
    [InlineData("https://images.example.com/photo.jpg")]
    public void Parse_ImageOrGif_ShouldDetectImageOrGifProvider(string url)
    {
        var result = MediaUrlParser.Parse(url);

        Assert.Equal(MediaProvider.ImageOrGif, result.Provider);
        Assert.Null(result.StartSeconds);
        Assert.Null(result.EndSeconds);
    }

    [Fact]
    public void Parse_NullOrEmpty_ShouldReturnEmptySafeDefaults()
    {
        var result = MediaUrlParser.Parse(null);
        Assert.Equal(MediaProvider.ImageOrGif, result.Provider);
        Assert.Equal(string.Empty, result.CleanUrl);
    }

    [Fact]
    public void BuildUrlWithTimestamps_ShouldCorrectlyAppendFragment()
    {
        var built = MediaUrlParser.BuildUrlWithTimestamps("https://youtu.be/test1234567", 12, 18);
        Assert.Equal("https://youtu.be/test1234567#t=12,18", built);
    }
}
