using Application.Constants.Communication;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.LocalResource;
using Domain.Enums.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class LocalResourceConfigExpertWidget : ComponentBase
{
    [Inject] public ISerializerService SerializerService { get; init; } = null!;
    [Inject] public IWebClientService WebClientService { get; init; } = null!;
    [Parameter] public LocalResourceSlim LocalResource { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public bool ConfigExpanded { get; set; }
    [Parameter] public string ConfigFilterText { get; set; } = string.Empty;
    [Parameter] public GameSlim Game { get; set; } = null!;
    [Parameter] public EventCallback<LocalResourceSlim> ResourceUpdate { get; set; }
    [Parameter] public EventCallback<LocalResourceSlim> ResourceDelete { get; set; }
    [Parameter] public EventCallback<LocalResourceSlim> OpenConfigEditor { get; set; }
    [Parameter] public EventCallback<LocalResourceSlim> ConfigAdd { get; set; }
    [Parameter] public EventCallback<ConfigurationItemSlim> ConfigUpdate { get; set; }
    [Parameter] public EventCallback<ConfigurationItemSlim> ConfigDelete { get; set; }


    private async Task UpdateLocalResource(LocalResourceSlim localResource)
    {
        await ResourceUpdate.InvokeAsync(localResource);
    }

    private async Task DeleteLocalResource(LocalResourceSlim localResource)
    {
        await ResourceDelete.InvokeAsync(localResource);
    }

    private async Task OpenInEditor(LocalResourceSlim localResource)
    {
        await OpenConfigEditor.InvokeAsync(localResource);
    }

    private async Task ExportConfig(LocalResourceSlim localResource)
    {
        var configExport = localResource.ContentType switch
        {
            ContentType.Json => SerializerService.SerializeJson(localResource.ConfigSets.Select(x => x.ToExport()).ToList()),
            ContentType.Ini => localResource.ConfigSets.ToIni().ToString(),
            ContentType.Xml => localResource.ConfigSets.ToXml()?.ToString() ?? string.Empty,
            _ => string.Join(Environment.NewLine, localResource.ConfigSets.ToRaw())
        };

        var exportName = $"{FileHelpers.SanitizeSecureFilename(localResource.Name)}.{localResource.ContentType.GetFileExtension()}";
        var mimeType = localResource.ContentType switch
        {
            ContentType.Json => DataConstants.MimeTypes.Json,
            ContentType.Xml => DataConstants.MimeTypes.OpenXml,
            _ => DataConstants.MimeTypes.Binary
        };
        var downloadResult = await WebClientService.InvokeFileDownload(configExport, exportName, mimeType);
        if (!downloadResult.Succeeded)
        {
            downloadResult.Messages.ForEach(x => Snackbar.Add(x));
            return;
        }

        Snackbar.Add($"Successfully Exported Config File: {exportName}");
    }

    private bool ConfigShouldBeShown(ConfigurationItemSlim item)
    {
        var shouldBeShown = item.FriendlyName.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase) ||
                            item.Value.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase);

        return shouldBeShown;
    }

    private async Task AddConfigItem(LocalResourceSlim localResource)
    {
        await ConfigAdd.InvokeAsync(localResource);
    }

    private async Task UpdateConfigItem(ConfigurationItemSlim item)
    {
        await ConfigUpdate.InvokeAsync(item);
    }

    private async Task DeleteConfigItem(ConfigurationItemSlim item)
    {
        await ConfigDelete.InvokeAsync(item);
    }

    private async Task InjectDynamicValue(ConfigurationItemSlim item, string value)
    {
        item.Value = value;
        await UpdateConfigItem(item);
    }
}