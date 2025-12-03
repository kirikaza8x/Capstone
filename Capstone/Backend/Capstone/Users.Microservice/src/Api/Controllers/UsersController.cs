using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Common.Commands;
using Shared.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Presentation.Common.Attributes;
using Shared.Application.DTOs;
using Users.Application.Features.Users.Queries;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Commands.RegisterUser;
using Users.Application.Features.Users.Commands.Login;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ApiControllerV2
    {
        public UserController(IMediator mediator) : base(mediator) { }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request, 
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
            if (result.IsFailure) return HandleResult(result);

            await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            return HandleResult(result); 
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request, 
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginUserCommand(request), cancellationToken);
            if (result.IsFailure) return HandleResult(result);

            await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            return HandleResult(result); // Return the login result with tokens
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequestDto request, 
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
            if (result.IsFailure) return HandleResult(result);

            await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            return HandleResult(result); // Return the refresh token result
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Guest,Customer")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromCurrentUser] CurrentUserDto user,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Guest,Customer")]
        public async Task<IActionResult> GetAll(
            [FromCurrentUser] CurrentUserDto user,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("me")]
        // [Authorize(Roles = "Admin,Guest,Customer")]
        public IActionResult GetCurrentUser(
            [SwaggerIgnoreModel]
            [FromCurrentUser] CurrentUserDto user)
        {
            return Ok(user);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}