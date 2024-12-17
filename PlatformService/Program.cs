using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataService;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
var env = builder.Environment;

if(env.IsProduction())
{
    Console.WriteLine("--> Using SqlServer DB");
    builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(Configuration.GetConnectionString("platformsConn")));
}
else
{
    Console.WriteLine("--> Using InMem DB");
    builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("InMem"));
}


builder.Services.AddScoped<IPlatformRepo,PlatformRepo>();
builder.Services.AddHttpClient<ICommandDataClient,HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient,MessageBusClient>();
builder.Services.AddGrpc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// Console.WriteLine($"--> CommandService Endpoint {Configuration}");
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

PrepDb.PrepPopulation(app,env.IsProduction());
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<GrpcPlatformService>();
app.MapGet("/Protos/platforms.proto", async context =>
{
    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
});

app.Run();

