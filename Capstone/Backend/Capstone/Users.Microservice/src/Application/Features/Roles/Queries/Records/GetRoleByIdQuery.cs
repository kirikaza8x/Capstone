using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Roles.Dtos;

public class GetRoleByIdQuery : IQuery<RoleResponseDto>
    {
        public Guid Id { get; }

        public GetRoleByIdQuery(Guid id)
        {
            Id = id;
        }
    }