using ApprovalWorkflow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.ErrorHandling;


//Load environment variable
Env.TraversePath().Load();


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IJwtgenerator, JwtGenerator>();


// Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// Db Context Register
string conneStr = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseNpgsql(connectionString: conneStr);
});


// Add MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<Program>();
});


// Add Controllers
builder.Services.AddControllers();


// Add authentication
var jwtKey = builder.Configuration["Jwt:Secret"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            key: Encoding.UTF8.GetBytes(jwtKey!)
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };

    option.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["access-token"];

            return Task.CompletedTask;
        }
    };
});



var app = builder.Build();


// Add Controller
app.MapControllers();


// Add Global Exception Handler
app.UseExceptionHandler();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthentication();
app.UseHttpsRedirection();


app.Run();
