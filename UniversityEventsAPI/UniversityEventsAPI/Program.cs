var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();   // .NET 9 OpenAPI dəstəyi

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
