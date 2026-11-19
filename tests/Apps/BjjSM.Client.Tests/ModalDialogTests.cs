using System.Reflection;
using System.Text.RegularExpressions;
using BjjSM.Client.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BjjSM.Client.Tests;

public class ModalDialogTests
{
    private sealed class FakeJsObjectReference : IJSObjectReference
    {
        public List<(string Identifier, object?[]? Args)> Invocations { get; } = [];
        public bool IsDisposed { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Invocations.Add((identifier, args));
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

    private sealed class ThrowingJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => throw new JSException("No JS runtime available");

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => throw new JSException("No JS runtime available");
    }

    private sealed class TestableModalDialog : ModalDialog
    {
        public Task TriggerAfterRenderAsync(bool firstRender = true) => OnAfterRenderAsync(firstRender);
    }

    private static async Task<string> RenderHtmlAsync<TComponent>(
        Dictionary<string, object?> parameters,
        IJSRuntime? jsRuntime = null) where TComponent : IComponent
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJSRuntime>(jsRuntime ?? new FakeJsRuntime(new FakeJsObjectReference()));
        await using var serviceProvider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(serviceProvider, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    [Fact]
    public async Task ModalDialog_WhenOpen_RendersNativeDialogWithMatchingAriaLabelledByAndDefaultCloseRequest()
    {
        var html = await RenderHtmlAsync<ModalDialog>(new Dictionary<string, object?>
        {
            [nameof(ModalDialog.IsOpen)] = true,
            [nameof(ModalDialog.Title)] = "Test Dialog Title"
        });

        Assert.Contains("<dialog", html);
        Assert.Contains("closedby=\"closerequest\"", html);
        Assert.Contains("Test Dialog Title", html);

        var ariaMatch = Regex.Match(html, @"aria-labelledby=""([^""]+)""");
        Assert.True(ariaMatch.Success, "Expected aria-labelledby attribute on <dialog>.");
        var titleId = ariaMatch.Groups[1].Value;

        Assert.Contains($"<h2 id=\"{titleId}\" class=\"bjj-dialog-title\">Test Dialog Title</h2>", html);
    }

    [Fact]
    public async Task ModalDialog_WhenLightDismissEnabled_RendersClosedByAny()
    {
        var html = await RenderHtmlAsync<ModalDialog>(new Dictionary<string, object?>
        {
            [nameof(ModalDialog.IsOpen)] = true,
            [nameof(ModalDialog.Title)] = "Lightbox",
            [nameof(ModalDialog.LightDismiss)] = true
        });

        Assert.Contains("<dialog", html);
        Assert.Contains("closedby=\"any\"", html);
    }

    [Fact]
    public async Task ModalDialog_WhenClosed_RendersNothing()
    {
        var html = await RenderHtmlAsync<ModalDialog>(new Dictionary<string, object?>
        {
            [nameof(ModalDialog.IsOpen)] = false,
            [nameof(ModalDialog.Title)] = "Hidden"
        });

        Assert.DoesNotContain("<dialog", html);
    }

    [Fact]
    public async Task ModalDialog_WhenShowHeaderFalse_RendersVisuallyHiddenHeadingMatchingAriaLabelledBy()
    {
        var html = await RenderHtmlAsync<ModalDialog>(new Dictionary<string, object?>
        {
            [nameof(ModalDialog.IsOpen)] = true,
            [nameof(ModalDialog.ShowHeader)] = false,
            [nameof(ModalDialog.AriaLabel)] = "Accessible Label"
        });

        var ariaMatch = Regex.Match(html, @"aria-labelledby=""([^""]+)""");
        Assert.True(ariaMatch.Success);
        var titleId = ariaMatch.Groups[1].Value;

        Assert.Contains($"<h2 id=\"{titleId}\" class=\"visually-hidden\">Accessible Label</h2>", html);
    }

    [Fact]
    public async Task ModalDialog_RequestCloseAsync_InvokesOnCloseAndClosesNativeDialog()
    {
        var module = new FakeJsObjectReference();
        var js = new FakeJsRuntime(module);
        var dialog = new TestableModalDialog();

        var jsProp = typeof(ModalDialog).GetProperty("JS", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(jsProp);
        jsProp.SetValue(dialog, js);

        var closedCount = 0;
        dialog.IsOpen = true;
        dialog.LightDismiss = true;
        dialog.OnClose = EventCallback.Factory.Create(this, () => closedCount++);

        await dialog.TriggerAfterRenderAsync();
        Assert.Contains(js.Invocations, i => i.Identifier == "import" && i.Args is ["./js/app-interop.js"]);
        Assert.Contains(module.Invocations, i => i.Identifier == "showModalDialog");

        await dialog.RequestCloseAsync();
        Assert.Equal(1, closedCount);
        Assert.Contains(module.Invocations, i => i.Identifier == "closeDialog");

        await dialog.DisposeAsync();
        Assert.True(module.IsDisposed);
    }

    [Fact]
    public async Task ModalDialog_WhenJsThrows_DoesNotCrash()
    {
        var dialog = new TestableModalDialog();
        var jsProp = typeof(ModalDialog).GetProperty("JS", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(jsProp);
        jsProp.SetValue(dialog, new ThrowingJsRuntime());

        var closed = false;
        dialog.IsOpen = true;
        dialog.OnClose = EventCallback.Factory.Create(this, () => closed = true);

        await dialog.TriggerAfterRenderAsync();
        await dialog.RequestCloseAsync();

        Assert.True(closed);
    }

    [Fact]
    public async Task ConfirmDialog_WhenOpen_FocusesSafeCancelButtonAndUsesCloseRequest()
    {
        var html = await RenderHtmlAsync<ConfirmDialog>(new Dictionary<string, object?>
        {
            [nameof(ConfirmDialog.IsOpen)] = true,
            [nameof(ConfirmDialog.Title)] = "Slet element",
            [nameof(ConfirmDialog.Message)] = "Er du sikker?",
            [nameof(ConfirmDialog.ConfirmText)] = "Slet",
            [nameof(ConfirmDialog.CancelText)] = "Annuller"
        });

        Assert.Contains("<dialog", html);
        Assert.Contains("closedby=\"closerequest\"", html);
        Assert.Contains("Er du sikker?", html);
        Assert.Matches(@"<button[^>]*autofocus[^>]*>\s*Annuller\s*</button>", html);
        Assert.DoesNotMatch(@"<button[^>]*autofocus[^>]*>\s*Slet\s*</button>", html);
    }

    [Fact]
    public async Task ConfirmDialog_InvokesOnConfirmAndOnCancelCallbacks()
    {
        var confirmDialog = new ConfirmDialog();
        var confirmed = false;
        var cancelled = false;

        confirmDialog.OnConfirm = EventCallback.Factory.Create(this, () => confirmed = true);
        confirmDialog.OnCancel = EventCallback.Factory.Create(this, () => cancelled = true);

        var handleConfirm = typeof(ConfirmDialog).GetMethod("HandleConfirmAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        var handleCancel = typeof(ConfirmDialog).GetMethod("HandleCancelAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handleConfirm);
        Assert.NotNull(handleCancel);

        await (Task)handleConfirm.Invoke(confirmDialog, null)!;
        await (Task)handleCancel.Invoke(confirmDialog, null)!;

        Assert.True(confirmed);
        Assert.True(cancelled);
    }
}
