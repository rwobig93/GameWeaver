using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Mappers.Lifecycle;
using Application.Models.Web;
using Application.Responses.Lifecycle;
using Application.Services.Lifecycle;
using Domain.Enums.Lifecycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.Lifecycle;

/// <summary>
/// API endpoints for application audit trails
/// </summary>
public static class AuditEndpoints
{
    /// <summary>
    /// Register API endpoints for audit
    /// </summary>
    /// <param name="app"></param>
    public static void MapEndpointsAudit(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.Lifecycle.Audit.GetAll, GetAllTrails).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Lifecycle.Audit.GetById, GetAuditTrailById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Lifecycle.Audit.GetByChangedBy, GetAuditTrailsByChangedBy).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Lifecycle.Audit.GetByRecordId, GetAuditTrailsByRecordId).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.Lifecycle.Audit.Delete, Delete).ApiVersionOne();
    }

    /// <summary>
    /// Get all audit trails
    /// </summary>
    /// <param name="auditService"></param>
    /// <returns>List of all audit trails</returns>
    [Authorize(PermissionConstants.Audit.View)]
    private static async Task<IResult<List<AuditTrailResponse>>> GetAllTrails(IAuditTrailService auditService)
    {
        try
        {
            var allAuditTrails = await auditService.GetAllAsync();
            if (!allAuditTrails.Succeeded)
                return await Result<List<AuditTrailResponse>>.FailAsync(allAuditTrails.Messages);

            return await Result<List<AuditTrailResponse>>.SuccessAsync(allAuditTrails.Data.ToResponses().ToList());
        }
        catch (Exception ex)
        {
            return await Result<List<AuditTrailResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get the specified audit trail
    /// </summary>
    /// <param name="id">GUID ID of the audit trail</param>
    /// <param name="auditService"></param>
    /// <returns>Detail regarding the specified audit trail</returns>
    [Authorize(PermissionConstants.Audit.View)]
    private static async Task<IResult<AuditTrailResponse>> GetAuditTrailById([FromQuery]Guid id, IAuditTrailService auditService)
    {
        try
        {
            var foundAuditTrail = await auditService.GetByIdAsync(id);
            if (!foundAuditTrail.Succeeded)
                return await Result<AuditTrailResponse>.FailAsync(foundAuditTrail.Messages);

            if (foundAuditTrail.Data is null)
                return await Result<AuditTrailResponse>.FailAsync(ErrorMessageConstants.InvalidValueError);

            return await Result<AuditTrailResponse>.SuccessAsync(foundAuditTrail.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<AuditTrailResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all audit trails where an entity was modified by a specific user
    /// </summary>
    /// <param name="id">GUID ID of the user modifying entities</param>
    /// <param name="auditService"></param>
    /// <returns>List of all audit trails where an entity was modified by the specified user</returns>
    [Authorize(PermissionConstants.Audit.View)]
    private static async Task<IResult<List<AuditTrailResponse>>> GetAuditTrailsByChangedBy([FromQuery]Guid id, IAuditTrailService auditService)
    {
        try
        {
            var auditTrails = await auditService.GetByChangedByIdAsync(id);
            if (!auditTrails.Succeeded)
                return await Result<List<AuditTrailResponse>>.FailAsync(auditTrails.Messages);

            return await Result<List<AuditTrailResponse>>.SuccessAsync(auditTrails.Data.ToResponses().ToList());
        }
        catch (Exception ex)
        {
            return await Result<List<AuditTrailResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all audit trails for a specific entity
    /// </summary>
    /// <param name="id">GUID ID of the entity that was modified</param>
    /// <param name="auditService"></param>
    /// <returns>List of all audit trails where the specified entity ID is the modified entity</returns>
    [Authorize(PermissionConstants.Audit.View)]
    private static async Task<IResult<List<AuditTrailResponse>>> GetAuditTrailsByRecordId([FromQuery]Guid id, IAuditTrailService auditService)
    {
        try
        {
            var auditTrails = await auditService.GetByRecordIdAsync(id);
            if (!auditTrails.Succeeded)
                return await Result<List<AuditTrailResponse>>.FailAsync(auditTrails.Messages);

            return await Result<List<AuditTrailResponse>>.SuccessAsync(auditTrails.Data.ToResponses().ToList());
        }
        catch (Exception ex)
        {
            return await Result<List<AuditTrailResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete audit records older than a specified threshold
    /// </summary>
    /// <param name="olderThan">
    /// Timeframe of records to keep, any records older than this will be deleted
    /// 
    /// Options:
    ///  - OneMonth
    ///  - ThreeMonths
    ///  - SixMonths
    ///  - OneYear
    ///  - TenYears
    /// </param>
    /// <param name="auditService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.Audit.DeleteOld)]
    private static async Task<IResult> Delete(CleanupTimeframe olderThan, IAuditTrailService auditService)
    {
        try
        {
            var deleteRequest = await auditService.DeleteOld(olderThan);
            if (!deleteRequest.Succeeded) return deleteRequest;
            return await Result.SuccessAsync("Successfully cleaned up old records!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
}