using Shared.Domain.Common.DDD;

namespace Users.Domain.Entities;

public class Role : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set;} = default!;

    private Role() { }

    public Role(string name)
    {
        Name = name;
    }
}
