using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserServiceContext>(options =>
         options.UseSqlite(@"Data Source=user.db"));

var app = builder.Build();
//var dbContext = new UserServiceContext(new DbContextOptions<UserServiceContext>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //dbContext.Database.EnsureCreated();
}

app.UseAuthorization();

app.MapControllers();

app.Run();