using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.LocalResource;

namespace GameWeaver.Components.GameServer;

public partial class LocalResourceConfigExpertWidget : ComponentBase
{
    [Parameter] public LocalResourceSlim LocalResource { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
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