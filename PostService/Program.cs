using Microsoft.EntityFrameworkCore;
using PostService;
using PostService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PostServiceContext>(options =>
            options.UseSqlite(@"Data Source=post.db"));
//options.UseInMemoryDatabase("post-db")) ;

builder.Services.AddSingleton<IntegrationEventListenerService>();
builder.Services
    .AddHostedService<IntegrationEventListenerService>(
        provider => provider.GetService<IntegrationEventListenerService>()
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

//Consume.ListenForIntegrationEvents();

app.Run();