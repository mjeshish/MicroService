using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.EventProcessing;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddHostedService<MessageBusSubscriber>();
builder.Services.AddSingleton<IEventProcessor,EventProcessor>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IPlatformDataClient,PlatformDataClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
PrepDb.PrepPopulation(app);
app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();


