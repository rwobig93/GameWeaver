namespace GameWeaver;

public partial class App
{
    private bool _firstRender;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _firstRender = firstRender;
        await Task.CompletedTask;
    }
}