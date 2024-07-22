using Domain.Contracts;
using Microsoft.AspNetCore.Components;

namespace Application.Services.System;

public interface IWebClientService
{
    Task<IResult<string>> GetClientTimezone();
    Task<IResult> InvokeFileDownload(string content, string fileName, string mimeType);
    Task<IResult> InvokeScrollToTop();
    Task<IResult> InvokeScrollToElement(string elementName);
    Task<IResult> InvokeScrollToBottom(string elementName);
    Task<IResult> InvokePlayAudio(string elementName);
    Task<IResult> InvokeClipboardCopy(string content);
    Task<IResult<bool>> GetImageUrlEnsured(ElementReference image, string fallbackUrl);
    Task<IResult> OpenExternalUrl(string url);
}