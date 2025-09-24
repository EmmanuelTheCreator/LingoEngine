using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor;

/// <summary>
/// Provides access to the JavaScript helper module used by AbstUI.
/// The resolver loads the underlying ES module on first use and
/// exposes strongly typed proxies for all exported functions so that
/// consumers do not have to deal with <see cref="IJSObjectReference"/>
/// directly.
/// </summary>
public class AbstUIScriptResolver : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private Task<IJSObjectReference>? _moduleTask;

    public AbstUIScriptResolver(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private Task<IJSObjectReference> GetModuleAsync()
        => _moduleTask ??= _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js").AsTask();

    public async ValueTask<ElementReference> CanvasCreateCanvas(int width, int height)
        => await (await GetModuleAsync()).InvokeAsync<ElementReference>("abstCanvas.createCanvas", width, height);

    public async ValueTask CanvasDisposeCanvas(ElementReference canvas)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.disposeCanvas", canvas);

    public async ValueTask CanvasAddToBody(ElementReference canvas)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.addCanvasToBody", canvas);

    public async ValueTask CanvasAddToElement(ElementReference element, ElementReference canvas)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.addCanvasToElement", element, canvas);

    public async ValueTask CanvasSetOffset(ElementReference canvas, double offsetX, double offsetY)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.setCanvasOffset", canvas, offsetX, offsetY);

    public async ValueTask CanvasSetVisible(ElementReference canvas, bool visible)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.setCanvasVisible", canvas, visible);

    public async ValueTask<IJSObjectReference> CanvasGetContext(ElementReference canvas, bool pixilated)
        => await (await GetModuleAsync()).InvokeAsync<IJSObjectReference>("abstCanvas.getContext", canvas, pixilated);

    public async ValueTask CanvasClear(IJSObjectReference ctx, string color, int width, int height)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.clear", ctx, color, width, height);

    public async ValueTask CanvasSetPixel(IJSObjectReference ctx, int x, int y, string color)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.setPixel", ctx, x, y, color);

    public async ValueTask CanvasDrawLine(IJSObjectReference ctx, double x1, double y1, double x2, double y2, string color, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawLine", ctx, x1, y1, x2, y2, color, width);

    public async ValueTask CanvasDrawRect(IJSObjectReference ctx, double x, double y, double w, double h, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawRect", ctx, x, y, w, h, color, filled, width);

    public async ValueTask CanvasDrawCircle(IJSObjectReference ctx, double x, double y, double radius, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawCircle", ctx, x, y, radius, color, filled, width);

    public async ValueTask CanvasDrawArc(IJSObjectReference ctx, double x, double y, double radius, double startDeg, double endDeg, string color, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawArc", ctx, x, y, radius, startDeg, endDeg, color, width);

    public async ValueTask CanvasDrawPolygon(IJSObjectReference ctx, double[] points, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawPolygon", ctx, points, color, filled, width);

    public async ValueTask CanvasDrawText(IJSObjectReference ctx, double x, double y, string text, string font, string color, int fontSize, string alignment, int letterSpacing = 0)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawText", ctx, x, y, text, font, color, fontSize, alignment, letterSpacing);

    public async ValueTask CanvasDrawPictureData(IJSObjectReference ctx, byte[] data, int width, int height, int x, int y)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawPictureData", ctx, data, width, height, x, y);

    public async ValueTask<byte[]> CanvasGetImageData(IJSObjectReference ctx, int width, int height)
        => await (await GetModuleAsync()).InvokeAsync<byte[]>("abstCanvas.getImageData", ctx, width, height);

    public async ValueTask CanvasSetGlobalAlpha(IJSObjectReference ctx, double alpha)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.setGlobalAlpha", ctx, alpha);

    public async ValueTask<IJSObjectReference> MediaCreateVideo(string id, string url, DotNetObjectReference<object> dotNetHelper)
        => await (await GetModuleAsync()).InvokeAsync<IJSObjectReference>("abstMedia.createVideo", id, url, dotNetHelper);

    public async ValueTask MediaPlayVideo(IJSObjectReference video)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.playVideo", video);

    public async ValueTask MediaPauseVideo(IJSObjectReference video)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.pauseVideo", video);

    public async ValueTask MediaStopVideo(IJSObjectReference video)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.stopVideo", video);

    public async ValueTask MediaSeekVideo(IJSObjectReference video, double seconds)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.seekVideo", video, seconds);

    public async ValueTask<double> MediaGetDuration(IJSObjectReference video)
        => await (await GetModuleAsync()).InvokeAsync<double>("abstMedia.getDuration", video);

    public async ValueTask<double> MediaGetCurrentTime(IJSObjectReference video)
        => await (await GetModuleAsync()).InvokeAsync<double>("abstMedia.getCurrentTime", video);

    public async ValueTask<IJSObjectReference> AudioCreate(string id, DotNetObjectReference<object> dotNetHelper)
        => await (await GetModuleAsync()).InvokeAsync<IJSObjectReference>("abstMedia.createAudio", id, dotNetHelper);

    public async ValueTask AudioPlay(IJSObjectReference audio, string url)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.playAudio", audio, url);

    public async ValueTask AudioPause(IJSObjectReference audio)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.pauseAudio", audio);

    public async ValueTask AudioStop(IJSObjectReference audio)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.stopAudio", audio);

    public async ValueTask AudioResume(IJSObjectReference audio)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.resumeAudio", audio);

    public async ValueTask AudioSeek(IJSObjectReference audio, double seconds)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.seekAudio", audio, seconds);

    public async ValueTask<double> AudioGetCurrentTime(IJSObjectReference audio)
        => await (await GetModuleAsync()).InvokeAsync<double>("abstMedia.getCurrentTimeAudio", audio);

    public async ValueTask AudioSetVolume(IJSObjectReference audio, double volume)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.setVolumeAudio", audio, volume);

    public async ValueTask MediaBeep()
        => await (await GetModuleAsync()).InvokeVoidAsync("abstMedia.beep");

    public async ValueTask SetCursor(string cursor)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIKey.setCursor", cursor);
    public async ValueTask<ScrollData> GetScrollPosition(string elementRef)
        => await (await GetModuleAsync()).InvokeAsync<ScrollData>("AbstScrollContainer.getScrollPosition", elementRef);

    public async ValueTask ShowBootstrapModal(string id)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIWindow.showBootstrapModal", id);

    public async ValueTask HideBootstrapModal(string id)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", id);

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is not null && _moduleTask.IsCompletedSuccessfully)
        {
            var module = await _moduleTask;
            await module.DisposeAsync();
        }
    }

    public class ScrollData
    {
        public double ScrollTop { get; set; }
        public double ScrollLeft { get; set; }
        public double ScrollHeight { get; set; }
        public double ClientHeight { get; set; }
    }
}

