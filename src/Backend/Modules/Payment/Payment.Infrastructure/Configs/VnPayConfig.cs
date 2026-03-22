using Shared.Infrastructure.Configs;

namespace Payment.Infrastructure.Configs
{
    public class VnPayConfig : ConfigBase
    {
        public string Url { get; set; } = string.Empty;
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string QueryDrUrl { get; set; } = string.Empty;
    }
}
