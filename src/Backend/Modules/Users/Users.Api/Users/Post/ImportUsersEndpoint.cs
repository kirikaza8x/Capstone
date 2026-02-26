using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Users.Commands.Import.Records;
namespace Users.Api.Users.Post
{
    public class ImportUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/users/import", async (
                IFormFile file,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ImportSheetUsersCommand(new FormFileUpload(file));

                var result = await sender.Send(command, cancellationToken);

                return result.ToOk($"Imported {result.Value} users successfully.");
            })
            .WithTags("Users")
            .WithName("ImportUsers")
            .WithSummary("Import users from Excel")
            .WithDescription("Uploads an Excel file and imports users into the system")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}
