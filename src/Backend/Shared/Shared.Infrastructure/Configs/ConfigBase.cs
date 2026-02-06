namespace Shared.Infrastructure.Configs
{
    public abstract class ConfigBase
    {
        // Convention: section name = class name without "Config"
        public virtual string SectionName => GetType().Name.Replace("Config", string.Empty);
    }
}
