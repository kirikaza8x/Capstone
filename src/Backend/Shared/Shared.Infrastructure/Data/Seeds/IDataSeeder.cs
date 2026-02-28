namespace Shared.Infrastructure.Data.Seeds;

public interface IDataSeeder 
{
    Task SeedAllAsync();
}

public interface IDataSeeder<T> : IDataSeeder where T : class 
{
}