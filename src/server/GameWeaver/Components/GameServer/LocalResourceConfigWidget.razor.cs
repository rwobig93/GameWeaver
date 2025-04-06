using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;

namespace GameWeaver.Components.GameServer;

public partial class LocalResourceConfigWidget : ComponentBase
{
    [Parameter] public LocalResourceSlim LocalResource { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public bool CanEdit { get; set; }
    [Parameter] public string ConfigFilterText { get; set; } = string.Empty;
    [Parameter] public EventCallback<ConfigurationItemSlim> ConfigUpdate { get; set; }
    [Parameter] public EventCallback<ConfigurationItemSlim> ConfigDelete { get; set; }


    private bool ConfigShouldBeShown(ConfigurationItemSlim item)
    {
        var shouldBeShown = item.FriendlyName.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase) ||
                            item.Value.Contains(ConfigFilterText, StringComparison.OrdinalIgnoreCase);

        return shouldBeShown;
    }

    private async Task UpdateConfigItem(ConfigurationItemSlim item)
    {
        await ConfigUpdate.InvokeAsync(item);
    }

    private async Task DeleteConfigItem(ConfigurationItemSlim item)
    {
        await ConfigDelete.InvokeAsync(item);
    }

    private void InjectDynamicValue(ConfigurationItemSlim item, string value)
    {
        item.Value = value;
    }
}