using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Common.Commands;
using Shared.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Presentation.Common.Attributes;
using Users.Application.Features.User.Dtos;
using Users.Application.Features.User.Commands.RegisterUser;
using Shared.Application.DTOs;
using Users.Application.Features.User.Commands.Login;
using Users.Application.Features.User.Queries;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ApiControllerV2
    {
        public UserController(IMediator mediator) : base(mediator) { }

        // ============================================
        // BEFORE: Manual handling (verbose)
        // ============================================
        // [HttpPost("register")]
        // [AllowAnonymous]
        // public async Task<IActionResult> Register_Old(
        //     [FromBody] RegisterRequestDto request, 
        //     CancellationToken cancellationToken)
        // {
        //     var result = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
        //     if (result.IsFailure) return HandleFailure(result);
        //
        //     var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
        //     if (commit.IsFailure) return HandleFailure(commit);
        //
        //     return Ok(result);
        // }

        // ============================================
        // AFTER: Using HandleResult (cleaner)
        // ============================================
        // ============================================
        // OPTION 1: Return command result (recommended)
        // ============================================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request, 
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
            if (result.IsFailure) return HandleResult(result);

            await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            return HandleResult(result); // Return the registration result, not commit
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

        // ============================================
        // OPTION 2: If you need to handle SaveChanges failures
        // ============================================
        // [HttpPost("register")]
        // [AllowAnonymous]
        // public async Task<IActionResult> Register_WithCommitHandling(
        //     [FromBody] RegisterRequestDto request, 
        //     CancellationToken cancellationToken)
        // {
        //     var result = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
        //     if (result.IsFailure) return HandleResult(result);
        //
        //     var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
        //     if (commit.IsFailure) return HandleResult(commit);
        //
        //     return HandleResult(result);
        // }

        // ============================================
        // For queries, even simpler - single line!
        // ============================================
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
        [Authorize(Roles = "Admin,Guest,Customer")]
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