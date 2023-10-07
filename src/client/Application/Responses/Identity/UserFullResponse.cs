﻿namespace Application.Responses.Identity;

public class UserFullResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string AuthState { get; set; } = null!;
    public string AccountType { get; init; } = null!;
    public List<ExtendedAttributeResponse> ExtendedAttributes { get; set; } = new();
    public List<PermissionResponse> Permissions { get; set; } = new();
}