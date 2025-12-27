using Shared.Application.Common.ResponseModel.Pagination;
using Shared.Application.DTOs;
using System.ComponentModel;

namespace Users.Application.Features.Roles.Dtos;

public class AssignRoleRequestDto
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
public class RoleResponseDto : BaseDto<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class RoleRequestDto
{
    [DefaultValue("Admin")]
    public string Name { get; set; } = default!;

    [DefaultValue("Administrator role with full permissions")]
    public string? Description { get; set; }
}

public class RoleFilterDto : PageFilterRequestDto
{
    
    [DefaultValue("Admin")]
    public string? Name { get; set; }

    [DefaultValue("Administrator role with full permissions")]
    public string? Description { get; set; }

    [DefaultValue("")]
    public string? SearchTerm { get; set; }

    // [DefaultValue(ProductStatus.Active)]
    // [JsonConverter(typeof(JsonStringEnumConverter))]
    // public ProductStatus? Status { get; set; } = ProductStatus.Active;
}