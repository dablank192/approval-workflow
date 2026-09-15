using ApprovalWorkflow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;


//Load environment variable
Env.TraversePath().Load();


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddOpenApi();


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



var app = builder.Build();


// Add Controller
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
