using ConfigsDB.Domain.Repositories;
using Shared.Domain.UnitOfWork;

namespace ConfigsDB.Domain.UnitOfWork
{
    /// <summary>
    /// Unit of Work for the Configuration Database context.
    /// Provides access to configuration-related repositories and transaction management.
    /// </summary>
    public interface IConfigSettingUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Repository for managing configuration settings with category and environment support.
        /// </summary>
        IConfigSettingRepository ConfigSettings { get; }
    }
}