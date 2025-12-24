using Identity.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Identity.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(options =>
{
    options.MinimumLevel.Information();
    options.WriteTo.Console();
    options.WriteTo.Seq("http://localhost:5341/");
});


builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));
});

builder.Services.AddCore(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();

