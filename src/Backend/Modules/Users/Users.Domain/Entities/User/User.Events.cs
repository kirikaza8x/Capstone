using Shared.Domain.DDD;
using Users.Domain.Events;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public partial class User
    {
        protected override void Apply(IDomainEvent @event)
        {
            if (@event is UserCreatedEvent e)
            {
                Id = e.UserId;
                Email = e.Email;
                UserName = e.UserName;
                Status = UserStatus.Active;
            }
        }
    }
}