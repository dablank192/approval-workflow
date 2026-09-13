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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
