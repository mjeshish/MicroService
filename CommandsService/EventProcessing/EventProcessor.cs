using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory,
                IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch(eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break;    

            }

        }       

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determining Event");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch(eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> could not determine event Type");
                    return EventType.Undetermined;    
            }

        }

        private void addPlatform(string PlatformPublishedMessage)
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var PlatformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(PlatformPublishedMessage);

                try
                {
                    var plat = _mapper.Map<Platform>(PlatformPublishedDto);
                    if(!repo.ExternalPlatformExists(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine("--> Platform added!");
                    }
                    else
                    {
                        Console.WriteLine("--> Platform already exists");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"--> Could not add platform to DB {ex.Message}");
                }
            }
        }
    }
     enum EventType
        {
            PlatformPublished,
            Undetermined
        }
}