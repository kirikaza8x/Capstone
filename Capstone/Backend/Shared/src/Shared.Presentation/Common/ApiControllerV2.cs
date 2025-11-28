using System.Net;
using Shared.Application.Common.ResponseModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SharedLibrary.Common
{
    [ApiController]
    public abstract class ApiControllerV2 : ControllerBase
    {
        protected readonly IMediator _mediator;

        protected ApiControllerV2(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(Result.Failure<T>(result.Error));
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(Result.Failure(result.Error));
        }
    }
}
