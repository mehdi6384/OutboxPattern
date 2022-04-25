using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserServiceContext>(options =>
            options.UseSqlite(@"Data Source=user.db"));

//options.UseInMemoryDatabase("user-db"));

builder.Services.AddSingleton<IntegrationEventSenderService>();
builder.Services.AddHostedService<IntegrationEventSenderService>(provider => provider.GetService<IntegrationEventSenderService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();