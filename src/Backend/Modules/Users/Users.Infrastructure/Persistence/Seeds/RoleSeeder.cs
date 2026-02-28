using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data.Seeds;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Persistence.Seeds;

public class RoleSeeder(UserModuleDbContext context) : IDataSeeder
{
    public async Task SeedAllAsync()
    {
        var existingRoles = await context.Roles
               .Select(r => r.Name)
               .ToListAsync();

        var rolesToAdd = new List<Role>();

        if (!existingRoles.Contains(Shared.Domain.Constants.Roles.Admin))
            rolesToAdd.Add(Role.Create(Shared.Domain.Constants.Roles.Admin, "System Administrator"));

        if (!existingRoles.Contains(Shared.Domain.Constants.Roles.Staff))
            rolesToAdd.Add(Role.Create(Shared.Domain.Constants.Roles.Staff, "Staff"));

        if (!existingRoles.Contains(Shared.Domain.Constants.Roles.Organizer))
            rolesToAdd.Add(Role.Create(Shared.Domain.Constants.Roles.Organizer, "Event Organizer"));

        if (!existingRoles.Contains(Shared.Domain.Constants.Roles.Attendee))
            rolesToAdd.Add(Role.Create(Shared.Domain.Constants.Roles.Attendee, "Attendee"));

        if (rolesToAdd.Count > 0)
        {
            context.Roles.AddRange(rolesToAdd);
            await context.SaveChangesAsync();
        }
    }
}