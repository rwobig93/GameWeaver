using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using Infrastructure.Services.GameServer;
using Microsoft.AspNetCore.Components;

namespace GameWeaver.Pages.GameServer;

public partial class HostView : ComponentBase
{
    [Parameter] public Guid HostId { get; set; } = Guid.Empty;

    [Inject] public IHostService HostService { get; set; } = null!;
    [Inject] public IGameServerService GameServerService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private HostSlim _host = new() { Id = Guid.Empty };
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private List<GameServerSlim> _runningGameservers = [];

    private bool _canEditHost;
    private bool _canViewGameServers;
    private bool _canDeleteHost;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetPermissions();
                await GetClientTimezone();
                await GetViewingHost();
                await GetGameServers();
                StateHasChanged();
            }
        }
        catch
        {
            StateHasChanged();
        }
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
        {
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task GetViewingHost()
    {
        var response = await HostService.GetByIdAsync(HostId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.Hosts.NotFound);
            _validIdProvided = false;
            StateHasChanged();
            return;
        }

        _host = response.Data;
        
        if (_host.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetGameServers()
    {
        if (!_canViewGameServers)
        {
            return;
        }

        _runningGameservers = [];
        var response = await GameServerService.GetByHostIdAsync(_host.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _runningGameservers = response.Data.ToList();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditHost = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Update);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
        _canDeleteHost = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Delete) || _host.OwnerId == _loggedInUserId;
    }
    
    private async Task Save()
    {
        if (!_canEditHost) return;
        
        var response = await HostService.UpdateAsync(_host.ToUpdateRequest(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        ToggleEditMode();
        await GetViewingHost();
        Snackbar.Add("Host successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private async Task ChangeOwnership()
    {
        if (_host.OwnerId != _loggedInUserId)
        {
            return;
        }
        
        // TODO: Implement ownership dialog
        await Task.CompletedTask;
    }

    private async Task DeleteHost()
    {
        if (!_canDeleteHost)
        {
            return;
        }
        
        // TODO: Implement host delete
        await Task.CompletedTask;
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.HostsDashboard);
    }

    private string CpuDisplay()
    {
        var cpuCount = _host.Cpus?.Count ?? 0;
        var firstCpu = _host.Cpus?.FirstOrDefault();
        var cpuName = firstCpu?.Name ?? "Unknown";
        var physicalCores = _host.Cpus?.Sum(x => x.CoreCount) ?? 0;
        var logicalCores = _host.Cpus?.Sum(x => x.LogicalProcessorCount) ?? 0;

        return $"{cpuCount}x {cpuName} w/ {physicalCores}physical & {logicalCores}logical";
    }

    private string GpuDisplay()
    {
        var gpuCount = _host.Gpus?.Count ?? 0;
        var firstGpu = _host.Gpus?.FirstOrDefault();
        var gpuName = firstGpu?.Name ?? "Unknown";
        var gpuRam = firstGpu?.AdapterRam ?? 0;

        return $"{gpuCount}x {gpuName} @ {gpuRam} VRAM";
    }

    private string MotherboardDisplay()
    {
        var motherboardCount = _host.Motherboards?.Count ?? 0;
        var firstMotherboard = _host.Motherboards?.FirstOrDefault();
        var motherboardManufacturer = firstMotherboard?.Manufacturer ?? "Generic";
        var motherboardProduct = firstMotherboard?.Product ?? "Unknown";

        return $"{motherboardCount}x {motherboardManufacturer} {motherboardProduct}";
    }

    private string RamDisplay()
    {
        var ramStickCount = _host.RamModules?.Count ?? 0;
        var firstStick = _host.RamModules?.FirstOrDefault();
        var ramTotal = _host.RamModules?.Sum(x => (double)x.Capacity) ?? 0;
        var firstStickSpeed = firstStick?.Speed ?? 0;

        return $"{ramStickCount}x {ramTotal} @ {firstStickSpeed}Mhz";
    }

    private string NetInterfaceDisplay()
    {
        var interfaceCount = _host.NetworkInterfaces?.Count ?? 0;
        var primaryInterface = _host.NetworkInterfaces.GetPrimaryInterface();
        var interfaceName = primaryInterface?.Name ?? "Unknown";
        var interfaceType = primaryInterface?.Type ?? "Generic";
        var interfaceAddress = primaryInterface?.IpAddresses.FirstOrDefault(x => !x.StartsWith("127.")) ?? "0.0.0.0";

        return $"{interfaceCount}x [{interfaceType}]{interfaceName} @ {interfaceAddress}";
    }

    private string StorageDisplay()
    {
        var storageCount = _host.Storage?.Count ?? 0;
        var freeSpace = _host.Storage?.Sum(x => (double) x.FreeSpace) ?? 0;
        var totalSpace = _host.Storage?.Sum(x => (double) x.TotalSpace) ?? 0;
        var usedStoragePercent = 100 - Math.Round(freeSpace / totalSpace * 100);

        return $"{storageCount}x @ {usedStoragePercent}% Used";
    }
}