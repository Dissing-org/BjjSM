using BjjSM.Client.Components;
using BjjSM.Client.Models;
using BjjSM.Modules.Memberships.Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BjjSM.Client.Tests;

public class BjjBeltBadgeTests
{
    private static async Task<string> RenderHtmlAsync(Dictionary<string, object?> parameters)
    {
        var services = new ServiceCollection();
        await using var serviceProvider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(serviceProvider, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<BjjBeltBadge>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    [Fact]
    public async Task BjjBeltBadge_WhenShowLabelFalse_RendersVisuallyHiddenRankTooltip()
    {
        var rank = new RankDto(BeltLadder.Adult, "Blue", AdultBelt.Blue, null, 2);

        var html = await RenderHtmlAsync(new Dictionary<string, object?>
        {
            [nameof(BjjBeltBadge.Rank)] = rank,
            [nameof(BjjBeltBadge.ShowLabel)] = false
        });

        Assert.DoesNotContain("bjj-belt-label", html);
        Assert.Matches(@"<span class=""visually-hidden""[^>]*>Adult Division - Blue Belt \(2 Stripes\)</span>", html);
    }

    [Fact]
    public async Task BjjBeltBadge_WhenShowLabelTrue_RendersVisibleLabelWithoutDuplicateVisuallyHiddenSpan()
    {
        var rank = new RankDto(BeltLadder.Adult, "Purple", AdultBelt.Purple, null, 1);

        var html = await RenderHtmlAsync(new Dictionary<string, object?>
        {
            [nameof(BjjBeltBadge.Rank)] = rank,
            [nameof(BjjBeltBadge.ShowLabel)] = true
        });

        Assert.Matches(@"<span class=""bjj-belt-label""[^>]*>Purple Belt \(1 Stripe\)</span>", html);
        Assert.DoesNotContain("visually-hidden", html);
    }
}
