using Application.Services.System;
using Domain.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Infrastructure.Services.System;

public class WebClientService : IWebClientService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger _logger;

    public WebClientService(IJSRuntime jsRuntime, ILogger logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<IResult<string>> GetClientTimezone()
    {
        try
        {
            var clientTimeZone = await _jsRuntime.InvokeAsync<string>("getClientTimeZone");
            if (string.IsNullOrWhiteSpace(clientTimeZone))
                return await Result<string>.FailAsync("GMT", "Failed to get a valid timezone from the current web client");

            return await Result<string>.SuccessAsync(data: clientTimeZone);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred attempting to get the web client timezone");
            return await Result<string>.FailAsync("GMT", $"Failed to get a valid timezone from the current web client: {ex.Message}");
        }
    }

    public async Task<IResult> InvokeFileDownload(string content, string fileName, string mimeType)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("Download", new
            {
                ByteArray = content,
                FileName = fileName,
                MimeType = mimeType
            });

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke file download: {ex.Message}");
        }
    }

    public async Task<IResult> InvokeScrollToTop()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("scrollToTop");

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke scroll to top: {ex.Message}");
        }
    }

    public async Task<IResult> InvokeScrollToElement(string elementName)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("scrollToFragment", new
            {
                elementId = elementName
            });

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke scroll to element: {ex.Message}");
        }
    }

    public async Task<IResult> InvokeScrollToBottom(string elementName)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("ScrollToBottom", new
            {
                elementName
            });

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke scroll to element: {ex.Message}");
        }
    }

    public async Task<IResult> InvokePlayAudio(string elementName)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("PlayAudio", new {elementName});

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke play audio: {ex.Message}");
        }
    }

    public async Task<IResult> InvokeClipboardCopy(string content)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("copyToClipboard", content);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke clipboard copy: {ex.Message}");
        }
    }

    public async Task<IResult<string>> GetImageUrlEnsured(ElementReference image, string fallbackUrl)
    {
        try
        {
            var imageSource = await _jsRuntime.InvokeAsync<string>("ensureImageExists", new
            {
                img = image,
                fallback = fallbackUrl
            });

            return await Result<string>.SuccessAsync(imageSource);
        }
        catch (Exception ex)
        {
            return await Result<string>.FailAsync(fallbackUrl, $"Failed to invoke image existence insurance: {ex.Message}");
        }
    }

    public async Task<IResult> OpenExternalUrl(string url)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("openExternalUrl", url);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failed to invoke external url open: {ex.Message}");
        }
    }
}