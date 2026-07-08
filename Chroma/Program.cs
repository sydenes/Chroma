using Chroma.Application;
using Chroma.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Chroma CRM API",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/api/auth/status", () => Results.Ok(new
{
    authenticated = false,
    message = "Authentication module placeholder. JWT identity will be added in the next iteration."
}));

app.UseAuthorization();
app.MapControllers();

app.Run();

