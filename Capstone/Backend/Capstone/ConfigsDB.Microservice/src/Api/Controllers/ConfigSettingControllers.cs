using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using ConfigsDB.Application.Features.ConfigSettings.Commands;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using MediatR;
using Shared.Presentation.Common.Attributes;
using Shared.Application.DTOs;

namespace ConfigsDB.API.Controllers
{
    [Route("api/[controller]")]
    public class ConfigSettingsController : ApiControllerV2
    {
        public ConfigSettingsController(IMediator mediator) : base(mediator) { }

        [HttpPost("create")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromCurrentUser] CurrentUserDto user,
            [FromBody] ConfigSettingRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateConfigSettingCommand(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("bulk-create")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCreate(
            [FromCurrentUser] CurrentUserDto user,
            [FromBody] BulkConfigSettingRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BulkCreateConfigSettingsCommand(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/update")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            [FromBody] UpdateConfigSettingRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateConfigSettingCommand(id, request), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/metadata")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMetadata(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            [FromBody] ChangeConfigSettingMetadataRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateConfigSettingMetadataCommand(id, request), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/mark-plaintext")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkPlainText(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkConfigAsPlainTextCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/mark-encrypted")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkEncrypted(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkConfigAsEncryptedCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/deactivate")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeactivateConfigSettingCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/activate")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ActivateConfigSettingCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
            [FromCurrentUser] CurrentUserDto user,
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteConfigSettingCommand(id), cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("bulk-delete")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete(
            [FromCurrentUser] CurrentUserDto user,
            [FromBody] List<Guid> ids,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BulkDeleteConfigSettingsCommand(ids), cancellationToken);
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
