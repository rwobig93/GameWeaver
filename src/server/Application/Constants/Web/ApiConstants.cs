using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Application.Constants.Web;

public static class ApiConstants
{
    public static ApiVersionSet? SupportsVersionOne { get; set; }
    public static ApiVersionSet? SupportsOneTwo { get; set; }
    public static readonly ApiVersion Version1 = new(1.0);
    public static readonly ApiVersion Version2 = new(2.0);
}