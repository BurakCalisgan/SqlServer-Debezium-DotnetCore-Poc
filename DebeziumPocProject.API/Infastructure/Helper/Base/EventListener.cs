using DebeziumPocProject.API.Infastructure.Interfaces;
using DebeziumPocProject.API.Infastructure.Settings;
using Microsoft.Extensions.Options;

namespace DebeziumPocProject.API.Infastructure.Helper.Base
{
    public class EventListener : EventListenerBase, IEventListener
    {
        public EventListener(
           IConsumerConnection connection,
           IOptions<TopicSettings> topicSettings,
           ILogger<EventListener> logger) : base(connection, topicSettings.Value, logger)
        { }
    }
}
