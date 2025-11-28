using System.ComponentModel;

namespace Shared.Application.Common.ResponseModel.Pagination;

public class PageFilterRequestDto
{
    [DefaultValue(1)]
    public int PageIndex { get; set; } 
    [DefaultValue(10)]
    public int PageSize { get; set; }
    [DefaultValue("CreatedAt")]
    public string? OrderBy { get; set; } 
    [DefaultValue(false)]
    public bool IsAscending { get; set; } 
}
