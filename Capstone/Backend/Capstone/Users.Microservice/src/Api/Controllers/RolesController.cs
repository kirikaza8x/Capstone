using Microsoft.AspNetCore.Mvc;
using Shared.Application.Common.Commands;
using Shared.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Presentation.Common.Attributes;
using Shared.Application.DTOs;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;
using Users.Application.Features.Roles.Queries;
using MediatR;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : ApiControllerV2
    {
        public RoleController(IMediator mediator) : base(mediator) { }

        [HttpPost("create")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromCurrentUser]  CurrentUserDto role,
            [FromBody] RoleRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateRoleCommand(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        //[Authorize(Roles = "Admin,Guest,Customer")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromCurrentUser] CurrentUserDto user,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
            return HandleResult(result);
        }
        [HttpPut("{id:guid}")]
        //[Authorize(Roles = "Admin,Guest,Customer")]
        public async Task<IActionResult> update(
            Guid id,
            [FromCurrentUser] CurrentUserDto user,
            [FromBody] RoleRequestDto role,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateRoleCommand(id,role), cancellationToken);
            return HandleResult(result);
        }


        [HttpPost("list")]
        //[Authorize(Roles = "Admin,Guest,Customer")]
        public async Task<IActionResult> Search(
            [FromBody] RoleFilterDto filter,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRoleListQuery(filter), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("assign")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(
            [FromBody] AssignRoleRequestDto requestDto,
            CancellationToken cancellationToken)
        {
            var command = new AssignRoleCommand(requestDto.UserId, requestDto.RoleId);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
