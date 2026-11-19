using System.Reflection;
using BjjSM.Client.Components;
using BjjSM.Client.Models;
using Microsoft.JSInterop;
using Xunit;

namespace BjjSM.Client.Tests;

public class WowAchievementToastTests
{
    private sealed class TestableWowAchievementToast : WowAchievementToast
    {
        public Task TriggerOnParametersSetAsync() => OnParametersSetAsync();
    }

    private sealed class FakeJsObjectReference : IJSObjectReference
    {
        public List<string> Invocations { get; } = [];
        public bool IsDisposed { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Invocations.Add(identifier);
            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeJsRuntime(FakeJsObjectReference module) : IJSRuntime
    {
        public List<(string Identifier, object?[]? Args)> Invocations { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Invocations.Add((identifier, args));
            if (identifier == "import" && typeof(TValue) == typeof(IJSObjectReference))
            {
                return ValueTask.FromResult((TValue)(object)module);
            }

            return ValueTask.FromResult(default(TValue)!);
        }
    }

    [Fact]
    public async Task OnParametersSetAsync_WhenAchievementShown_ImportsAppInteropAndPlaysFanfareWithoutEval()
    {
        var module = new FakeJsObjectReference();
        var js = new FakeJsRuntime(module);
        var toast = new TestableWowAchievementToast();

        var jsProperty = typeof(WowAchievementToast).GetProperty("JS", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(jsProperty);
        jsProperty.SetValue(toast, js);

        toast.Current = new Achievement
        {
            Id = "first-point",
            Title = "First Point",
            Description = "Invested your first talent point",
            Icon = "🥋",
            Points = 10,
            Category = "Basics"
        };

        await toast.TriggerOnParametersSetAsync();

        Assert.Contains(js.Invocations, i => i.Identifier == "import" && i.Args is ["./js/app-interop.js"]);
        Assert.DoesNotContain(js.Invocations, i => i.Identifier == "eval");
        Assert.Contains("playAchievementFanfare", module.Invocations);

        await toast.DisposeAsync();
        Assert.True(module.IsDisposed);
    }
}
