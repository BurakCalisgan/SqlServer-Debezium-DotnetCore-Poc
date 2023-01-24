using Confluent.Kafka;
using DebeziumPocProject.API.Infastructure.Settings;

namespace DebeziumPocProject.API.Infastructure.Helper
{
    public interface IConsumerConnection
    {
        IConsumer<string, string> GetListenerConsumer(TopicSettings config);
        void StartReadingStatus();
        bool GetReadingStatus();
    }
}
