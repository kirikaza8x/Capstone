using Shared.Application.Common.ResponseModel.Pagination;
using Shared.Application.DTOs;
using System.ComponentModel;

namespace ConfigsDB.Application.Features.ConfigSettings.Dtos
{
    /// <summary>
    /// Request DTO for creating a new configuration setting.
    /// </summary>
    public class ConfigSettingRequestDto
    {
        [DefaultValue("ExpiryDays")]
        public string Key { get; set; } = default!;

        [DefaultValue("30")]
        public string Value { get; set; } = default!;

        [DefaultValue("JWT")]
        public string Category { get; set; } = default!;

        [DefaultValue("Global")]
        public string Environment { get; set; } = "Global";

        [DefaultValue("JWT token expiry in days")]
        public string? Description { get; set; }

        [DefaultValue(false)]
        public bool IsEncrypted { get; set; } = false;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request DTO for updating an existing configuration setting.
    /// </summary>
    public class UpdateConfigSettingRequestDto
    {
        [DefaultValue("45")]
        public string Value { get; set; } = default!;

        [DefaultValue("Updated JWT token expiry")]
        public string? Description { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request DTO for changing category or environment (rare operations).
    /// </summary>
    public class ChangeConfigSettingMetadataRequestDto
    {
        [DefaultValue("Authentication")]
        public string? Category { get; set; }

        [DefaultValue("Dev")]
        public string? Environment { get; set; }
    }

    /// <summary>
    /// Response DTO for configuration setting.
    /// </summary>
    public class ConfigSettingResponseDto : BaseDto<Guid>
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Environment { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsEncrypted { get; set; }
    }

    /// <summary>
    /// Simplified response DTO without audit fields (for public APIs).
    /// </summary>
    public class ConfigSettingSimpleResponseDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Environment { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsEncrypted { get; set; }
    }

    /// <summary>
    /// Filter DTO for querying configuration settings with pagination.
    /// </summary>
    public class ConfigSettingFilterDto : PageFilterRequestDto
    {
        [DefaultValue("ExpiryDays")]
        public string? Key { get; set; }

        [DefaultValue("JWT")]
        public string? Category { get; set; }

        [DefaultValue("Global")]
        public string? Environment { get; set; }

        [DefaultValue(true)]
        public bool? IsActive { get; set; }

        [DefaultValue(false)]
        public bool? IsEncrypted { get; set; }

        [DefaultValue("")]
        public string? SearchTerm { get; set; }

        [DefaultValue(null)]
        public DateTime? CreatedAfter { get; set; }

        [DefaultValue(null)]
        public DateTime? ModifiedAfter { get; set; }

        [DefaultValue("")]
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Response DTO for category summary (admin dashboard).
    /// </summary>
    public class ConfigCategorySummaryDto
    {
        public string Category { get; set; } = default!;
        public int TotalConfigs { get; set; }
        public int ActiveConfigs { get; set; }
        public int EncryptedConfigs { get; set; }
        public DateTime? LastModified { get; set; }
    }

    /// <summary>
    /// Response DTO for environment summary.
    /// </summary>
    public class ConfigEnvironmentSummaryDto
    {
        public string Environment { get; set; } = default!;
        public int TotalConfigs { get; set; }
        public int ActiveConfigs { get; set; }
        public List<string> Categories { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for bulk config operations.
    /// </summary>
    public class BulkConfigSettingRequestDto
    {
        public List<ConfigSettingRequestDto> ConfigSettings { get; set; } = new();
    }

    /// <summary>
    /// Response DTO for resolved config value (with environment fallback).
    /// </summary>
    public class ResolvedConfigValueDto
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string ResolvedFrom { get; set; } = default!; // "Dev", "Global", etc.
        public bool IsEncrypted { get; set; }
    }

    /// <summary>
    /// Request DTO for getting all configs in a category with resolution.
    /// </summary>
    public class GetCategoryConfigsRequestDto
    {
        [DefaultValue("JWT")]
        public string Category { get; set; } = default!;

        [DefaultValue("Dev")]
        public string Environment { get; set; } = "Global";
    }

    /// <summary>
    /// Response DTO for category configs with resolution.
    /// </summary>
    public class CategoryConfigsResponseDto
    {
        public string Category { get; set; } = default!;
        public string Environment { get; set; } = default!;
        public Dictionary<string, ResolvedConfigValueDto> Configs { get; set; } = new();
    }
}