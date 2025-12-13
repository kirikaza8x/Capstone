using Shared.Domain.Common.DDD;

namespace ConfigsDB.Domain.Entities
{
    /// <summary>
    /// Represents a configuration setting that can be overridden per environment.
    /// Supports categorization for easy grouping (e.g., JWT, Database, Email).
    /// </summary>
    public class ConfigSetting : AggregateRoot<Guid>
    {
        public string Key { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public string Environment { get; private set; } = "Global";
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsEncrypted { get; private set; } = false;

        // EF Core requires a parameterless constructor
        private ConfigSetting() { }

        /// <summary>
        /// Creates a new configuration setting.
        /// </summary>
        /// <param name="key">Unique configuration key (e.g., "ExpiryDays", "ConnectionString")</param>
        /// <param name="value">Configuration value</param>
        /// <param name="category">Category for grouping (e.g., "JWT", "Database", "Email")</param>
        /// <param name="environment">Environment scope (Global, Dev, Staging, Prod)</param>
        /// <param name="description">Optional description for documentation</param>
        /// <param name="isEncrypted">Whether the value is encrypted (for sensitive data)</param>
        /// <param name="createdBy">User who created this config (optional for system-created configs)</param>
        /// <returns>New ConfigSetting instance</returns>
        public static ConfigSetting Create(
            string key,
            string value,
            string category,
            string environment = "Global",
            string? description = null,
            bool isEncrypted = false,
            string? createdBy = null)
        {
            ValidateKey(key);
            ValidateValue(value);
            ValidateCategory(category);
            ValidateEnvironment(environment);

            var config = new ConfigSetting
            {
                Id = Guid.NewGuid(),
                Key = key.Trim(),
                Value = value,
                Category = category.Trim(),
                Environment = environment.Trim(),
                Description = description?.Trim(),
                IsEncrypted = isEncrypted,
                IsActive = true
            };

            // Set audit fields from base Entity class
            if (!string.IsNullOrEmpty(createdBy))
            {
                config.SetCreated(createdBy);
            }
            else
            {
                // For system-created configs without a specific user
                config.CreatedAt = DateTime.UtcNow;
                config.CreatedBy = "System";
            }

            // RaiseEvent(new ConfigSettingCreatedEvent(
            //     config.Id, 
            //     config.Key, 
            //     config.Category, 
            //     config.Environment,
            //     config.CreatedAt.Value
            // ));

            return config;
        }

        /// <summary>
        /// Updates the configuration value.
        /// </summary>
        public void UpdateValue(string value, string? modifiedBy = null)
        {
            ValidateValue(value);

            var oldValue = Value;
            Value = value;

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }

            // RaiseEvent(new ConfigSettingValueChangedEvent(
            //     Id, 
            //     Key, 
            //     oldValue, 
            //     value,
            //     ModifiedAt.Value
            // ));
        }

        /// <summary>
        /// Updates the description.
        /// </summary>
        public void UpdateDescription(string? description, string? modifiedBy = null)
        {
            Description = description?.Trim();

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }
        }

        /// <summary>
        /// Changes the category of this configuration.
        /// </summary>
        public void ChangeCategory(string category, string? modifiedBy = null)
        {
            ValidateCategory(category);

            var oldCategory = Category;
            Category = category.Trim();

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }

            // RaiseEvent(new ConfigSettingCategoryChangedEvent(
            //     Id, 
            //     Key, 
            //     oldCategory, 
            //     category,
            //     ModifiedAt.Value
            // ));
        }

        /// <summary>
        /// Changes the environment scope.
        /// </summary>
        public void ChangeEnvironment(string environment, string? modifiedBy = null)
        {
            ValidateEnvironment(environment);

            var oldEnvironment = Environment;
            Environment = environment.Trim();

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }

            // RaiseEvent(new ConfigSettingEnvironmentChangedEvent(
            //     Id, 
            //     Key, 
            //     oldEnvironment, 
            //     environment,
            //     ModifiedAt.Value
            // ));
        }

        /// <summary>
        /// Activates or deactivates the configuration.
        /// </summary>
        public void SetActive(bool isActive, string? modifiedBy = null)
        {
            if (IsActive == isActive) return;

            IsActive = isActive;

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }

            // RaiseEvent(new ConfigSettingStatusChangedEvent(
            //     Id, 
            //     Key, 
            //     isActive,
            //     ModifiedAt.Value
            // ));
        }

        /// <summary>
        /// Marks the value as encrypted (useful for sensitive data like connection strings).
        /// </summary>
        public void MarkAsEncrypted(string? modifiedBy = null)
        {
            IsEncrypted = true;

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }
        }

        /// <summary>
        /// Marks the value as plain text.
        /// </summary>
        public void MarkAsPlainText(string? modifiedBy = null)
        {
            IsEncrypted = false;

            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModified(modifiedBy);
            }
            else
            {
                ModifiedAt = DateTime.UtcNow;
                ModifiedBy = "System";
            }
        }

        /// <summary>
        /// Soft delete by deactivating.
        /// </summary>
        public void Deactivate(string? modifiedBy = null)
        {
            SetActive(false, modifiedBy);
        }

        /// <summary>
        /// Reactivate a previously deactivated config.
        /// </summary>
        public void Activate(string? modifiedBy = null)
        {
            SetActive(true, modifiedBy);
        }

        /// <summary>
        /// Soft delete using base Entity method.
        /// </summary>
        public void SoftDelete(string modifiedBy)
        {
            Delete(modifiedBy); // Calls base Entity.Delete()
        }

        #region Validation Methods

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Configuration key cannot be empty or whitespace.", nameof(key));

            if (key.Length > 200)
                throw new ArgumentException("Configuration key cannot exceed 200 characters.", nameof(key));
        }

        private static void ValidateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Configuration value cannot be empty or whitespace.", nameof(value));

            if (value.Length > 2000)
                throw new ArgumentException("Configuration value cannot exceed 2000 characters.", nameof(value));
        }

        private static void ValidateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Configuration category cannot be empty or whitespace.", nameof(category));

            if (category.Length > 100)
                throw new ArgumentException("Configuration category cannot exceed 100 characters.", nameof(category));
        }

        private static void ValidateEnvironment(string environment)
        {
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("Configuration environment cannot be empty or whitespace.", nameof(environment));

            if (environment.Length > 50)
                throw new ArgumentException("Configuration environment cannot exceed 50 characters.", nameof(environment));

            var validEnvironments = new[] { "Global", "Dev", "Development", "Staging", "Prod", "Production", "Test" };
            if (!validEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid environment '{environment}'. Valid values: {string.Join(", ", validEnvironments)}", 
                    nameof(environment));
            }
        }

        #endregion

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event sourcing logic if needed
            // switch (@event)
            // {
            //     case ConfigSettingCreatedEvent created:
            //         Id = created.Id;
            //         Key = created.Key;
            //         Category = created.Category;
            //         Environment = created.Environment;
            //         CreatedAt = created.CreatedAt;
            //         break;
            //     case ConfigSettingValueChangedEvent valueChanged:
            //         Value = valueChanged.NewValue;
            //         ModifiedAt = valueChanged.ChangedAt;
            //         break;
            //     // Add more event handlers as needed
            // }
        }

        public override string ToString()
        {
            return $"[{Category}] {Key} = {(IsEncrypted ? "***ENCRYPTED***" : Value)} ({Environment})";
        }
    }
}