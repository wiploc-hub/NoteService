using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NoteServiceApi.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notes WebAPI",
        Version = "v1",
        Description = "API for accessing Notes"
    });

    // Include XML comments for API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<INoteService, NoteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes WebAPI V1");
        c.RoutePrefix = string.Empty; // Make Swagger UI available at app root
    });
}

using (var scope = app.Services.CreateScope())
{
    var dbService2 = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
    await dbService2.Configure();
}

app.UseHttpsRedirection();

app.UseAuthorization();
try
{
    app.MapControllers();
}
catch (Exception ex)
{
    var msg = ex.Message;
}

app.Run();
