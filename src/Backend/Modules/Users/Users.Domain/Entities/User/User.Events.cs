using Shared.Domain.DDD;
using Users.Domain.Enums;
using Users.Domain.Events;

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
