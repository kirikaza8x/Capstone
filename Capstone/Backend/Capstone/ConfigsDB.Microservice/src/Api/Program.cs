using ConfigsDB.Api;
using ConfigsDB.Application;
using ConfigsDB.Domain;
using Shared.Presentation.Configs.Swagger;


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
    app.UseSwaggerUI(c =>
        SwaggerUIConfig.ConfigureSwaggerUI(
            c,
            documentName: "v1",        
            serviceName: "Config API",   
            routePrefix: "config-docs"   
        )
    );
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


