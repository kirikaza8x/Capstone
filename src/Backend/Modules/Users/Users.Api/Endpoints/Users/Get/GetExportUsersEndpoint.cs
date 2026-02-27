using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Carter;

namespace Users.Api.Users.Get
{
    public class ExportUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users/export", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new ExportSheetUsersQuery();
                var result = await sender.Send(query, cancellationToken);
                return Results.File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
            })
            .WithTags("Users")
            .WithName("ExportUsers")
            .WithSummary("Export users to Excel")
            .WithDescription("Exports all users into an Excel file")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}
