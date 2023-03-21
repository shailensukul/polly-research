using Unreliable_Service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

using var context = new SampleDbContext();
context.Database.EnsureCreated();
context.Database.Migrate();
builder.Services.AddDbContext<SampleDbContext>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Shai's Unreliable Web Service",
        Description = "Do not trust this service",
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    // https://stackoverflow.com/questions/68498285/how-to-create-a-new-address-for-each-controller-in-swagger
    x.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Shai's Unreliable Web Service",
        Description = "Do not trust this service",
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();