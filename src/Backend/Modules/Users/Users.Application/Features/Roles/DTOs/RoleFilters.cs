using System.ComponentModel;
using Shared.Application.Dtos.Queries;
using Users.Domain.Enums;

namespace Users.Application.Features.Roles.Dtos;

public sealed record RoleFilterRequestDto : PagedRequestDto
{
    [DefaultValue("")]
    public string? Name { get; set; } = string.Empty;

    [DefaultValue("")]
    public string? Description { get; set; } = string.Empty;
}