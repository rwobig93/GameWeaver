using Domain.Contracts;

namespace Application.Services;

public interface IHostService
{
    Task<IResult> SaveSettings(string settingsSectionName, object updatedSection);
}