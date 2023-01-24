using DebeziumPocProject.API.Application;
using DebeziumPocProject.API.Infastructure.Helper;
using DebeziumPocProject.API.Infastructure.Helper.Base;
using DebeziumPocProject.API.Infastructure.Interfaces;
using DebeziumPocProject.API.Infastructure.Settings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
builder.Services.Configure<TopicSettings>(builder.Configuration.GetSection(nameof(TopicSettings)));
builder.Services.AddSingleton<IConsumerConnection, ConsumerConnection>();
builder.Services.AddTransient<IEventListener, EventListener>();

builder.Services.AddHostedService<ConsumerHostedService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
