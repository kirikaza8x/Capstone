namespace Marketing.Domain.Enums;

/// <summary>
/// Supported external platforms for post distribution.
/// </summary>
public enum ExternalPlatform
{
    /// <summary>
    /// Unknown or unspecified platform
    /// </summary>
    Unknown = 0,
    
    // ==================== Social Media ====================
    
    /// <summary>
    /// Facebook Page/Profile
    /// </summary>
    Facebook = 1,
    
    /// <summary>
    /// Twitter / X
    /// </summary>
    Twitter = 2,
    
    /// <summary>
    /// LinkedIn Page/Profile
    /// </summary>
    LinkedIn = 3,
    
    /// <summary>
    /// Instagram Business Account
    /// </summary>
    Instagram = 4,
    
    /// <summary>
    /// TikTok Business Account
    /// </summary>
    TikTok = 5,
    
    /// <summary>
    /// YouTube Channel
    /// </summary>
    YouTube = 6,

    /// <summary>
    /// Threads (Meta's text-based social platform)
    /// </summary>
    Threads = 7,

    // ==================== Publishing Platforms ====================

    /// <summary>
    /// Medium publication
    /// </summary>
    Medium = 10,
    
    /// <summary>
    /// DEV.to community
    /// </summary>
    DevTo = 11,
    
    /// <summary>
    /// Hashnode blog
    /// </summary>
    Hashnode = 12,
    
    // ==================== Messaging Apps ====================
    
    /// <summary>
    /// Telegram Channel/Bot
    /// </summary>
    Telegram = 20,
    
    /// <summary>
    /// WhatsApp Business API
    /// </summary>
    WhatsApp = 21,
    
    /// <summary>
    /// Discord Server/Webhook
    /// </summary>
    Discord = 22,
    
    // ==================== Custom ====================
    
    /// <summary>
    /// Custom/Other platform (use PlatformMetadata for details)
    /// </summary>
    Custom = 99
}
