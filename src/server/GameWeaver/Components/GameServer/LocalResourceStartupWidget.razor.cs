using Application.Models.GameServer.Game;
using Application.Models.GameServer.LocalResource;

namespace GameWeaver.Components.GameServer;

public partial class LocalResourceStartupWidget : ComponentBase
{
    [Parameter] public LocalResourceSlim LocalResource { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public GameSlim Game { get; set; } = null!;
    [Parameter] public EventCallback<LocalResourceSlim> ResourceUpdate { get; set; }
    [Parameter] public EventCallback<LocalResourceSlim> OpenScriptEditor { get; set; }
    [Parameter] public EventCallback<LocalResourceSlim> ResourceDelete { get; set; }


    private async Task UpdateLocalResource(LocalResourceSlim localResource)
    {
        await ResourceUpdate.InvokeAsync(localResource);
    }

    private async Task OpenInEditor(LocalResourceSlim localResource)
    {
        await OpenScriptEditor.InvokeAsync(localResource);
    }

    private async Task DeleteLocalResource(LocalResourceSlim localResource)
    {
        await ResourceDelete.InvokeAsync(localResource);
    }

    private void InjectDynamicValue(LocalResourceSlim resource, string value)
    {
        resource.Args += $" {value}";
    }
}