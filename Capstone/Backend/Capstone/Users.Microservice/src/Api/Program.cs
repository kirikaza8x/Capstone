using Users.Application;
using Shared.Presentation.Configs.Swagger;
using Users.Api;
using Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(SwaggerUIConfig.ConfigureSwaggerUI);
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


