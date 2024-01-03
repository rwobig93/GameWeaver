namespace Domain.Contracts;

public interface IWebResult : Microsoft.AspNetCore.Http.IResult
{
    IResult Result { get; set; }
}