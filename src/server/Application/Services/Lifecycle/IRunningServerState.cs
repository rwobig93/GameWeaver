namespace Application.Services.Lifecycle;

public interface IRunningServerState
{
    public bool IsRunningInDebugMode { get; }
    public string ApplicationName { get; }
    public Guid SystemUserId { get; }
    public Version ApplicationVersion { get; }
    public string PublicIp { get; }

    public void UpdateServerState();
    public void UpdateSystemUserId(Guid systemUserId);
    public Task UpdatePublicIp();
}