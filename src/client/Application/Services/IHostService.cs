using Application.Settings;
using Domain.Contracts;

namespace Application.Services;

public interface IHostService
{
    Task<IResult> SaveSettings(IAppSettingsSection settingsSection);
}