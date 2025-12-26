using Identity.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Identity.Core;
using Serilog;
using Identity.Core.Domain.Entities;
using Identity.Core.Data;
using Microsoft.AspNetCore.Identity;
using Identity.Core.Exceptions;

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

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opt => 
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireLowercase = true;   
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequiredUniqueChars = 3;
    opt.Password.RequiredLength = 6;

    opt.User.RequireUniqueEmail = true;

    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddErrorDescriber<IdentityTranslatedErrors>()
.AddDefaultTokenProviders();

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

