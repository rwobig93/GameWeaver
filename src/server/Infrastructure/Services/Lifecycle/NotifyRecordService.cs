using Application.Mappers.Lifecycle;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Lifecycle;
using Domain.Contracts;

namespace Infrastructure.Services.Lifecycle;

public class NotifyRecordService : INotifyRecordService
{
    private readonly INotifyRecordRepository _notifyRepository;

    public NotifyRecordService(INotifyRecordRepository notifyRepository)
    {
        _notifyRepository = notifyRepository;
    }

    public async Task<IResult<IEnumerable<NotifyRecordSlim>>> GetAllAsync()
    {
        try
        {
            var records = await _notifyRepository.GetAllAsync();
            if (!records.Succeeded)
            {
                return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(records.ErrorMessage);
            }

            return await Result<IEnumerable<NotifyRecordSlim>>.SuccessAsync(records.Result?.ToSlims() ?? new List<NotifyRecordSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<NotifyRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _notifyRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.SuccessAsync([]);
            }

            return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.SuccessAsync(
                response.Result.Data.ToSlims(),
                response.Result.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<NotifyRecordSlim?>> GetByIdAsync(int id)
    {
        try
        {
            var record = await _notifyRepository.GetByIdAsync(id);
            if (!record.Succeeded)
            {
                return await Result<NotifyRecordSlim?>.FailAsync(record.ErrorMessage);
            }

            return await Result<NotifyRecordSlim?>.SuccessAsync(record.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<NotifyRecordSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<NotifyRecordSlim>>> GetByEntityIdAsync(Guid id, int recordCount)
    {
        try
        {
            var record = await _notifyRepository.GetByEntityIdAsync(id, recordCount);
            if (!record.Succeeded)
            {
                return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(record.ErrorMessage);
            }

            return await Result<IEnumerable<NotifyRecordSlim>>.SuccessAsync(record.Result?.ToSlims() ?? Array.Empty<NotifyRecordSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<NotifyRecordSlim>>> GetAllByEntityIdAsync(Guid id)
    {
        try
        {
            var record = await _notifyRepository.GetAllByEntityIdAsync(id);
            if (!record.Succeeded)
            {
                return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(record.ErrorMessage);
            }

            return await Result<IEnumerable<NotifyRecordSlim>>.SuccessAsync(record.Result?.ToSlims() ?? Array.Empty<NotifyRecordSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> CreateAsync(NotifyRecordCreate createObject)
    {
        try
        {
            var response = await _notifyRepository.CreateAsync(createObject);
            if (!response.Succeeded)
            {
                return await Result<int>.FailAsync(response.ErrorMessage);
            }

            return await Result<int>.SuccessAsync(response.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<NotifyRecordSlim>>> SearchAsync(string searchTerm)
    {
        try
        {
            var records = await _notifyRepository.SearchAsync(searchTerm);
            if (!records.Succeeded)
            {
                return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(records.ErrorMessage);
            }

            return await Result<IEnumerable<NotifyRecordSlim>>.SuccessAsync(records.Result?.ToSlims() ?? new List<NotifyRecordSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<NotifyRecordSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _notifyRepository.SearchPaginatedAsync(searchTerm, pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.SuccessAsync([]);
            }

            return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.SuccessAsync(
                response.Result.Data.ToSlims(),
                response.Result.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<NotifyRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> DeleteOlderThan(DateTime olderThan)
    {
        try
        {
            var response = await _notifyRepository.DeleteOlderThan(olderThan);
            if (!response.Succeeded)
            {
                return await Result<int>.FailAsync(response.ErrorMessage);
            }

            return await Result<int>.SuccessAsync(response.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> DeleteAllForEntityId(Guid id)
    {
        try
        {
            var response = await _notifyRepository.DeleteAllForEntityId(id);
            if (!response.Succeeded)
            {
                return await Result<int>.FailAsync(response.ErrorMessage);
            }

            return await Result<int>.SuccessAsync(response.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
}