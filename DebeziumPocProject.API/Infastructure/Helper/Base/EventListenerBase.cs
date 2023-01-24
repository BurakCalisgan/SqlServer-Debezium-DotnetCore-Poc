using Confluent.Kafka;
using DebeziumPocProject.API.Infastructure.Interfaces;
using DebeziumPocProject.API.Infastructure.Models;
using DebeziumPocProject.API.Infastructure.Settings;
using Newtonsoft.Json;
using System.Linq;

namespace DebeziumPocProject.API.Infastructure.Helper.Base
{
    public class EventListenerBase : IEventListener
    {
        #region Members

        private readonly TopicSettings _topicSettings;
        private readonly ILogger<EventListenerBase> _logger;

        private readonly IConsumer<string, string> _consumer;

        private int _currentReadCount = 0;
        private List<TopicPartitionOffset> _topicPartitionOffsets;

        #endregion Members
        private long offsetInicial = 0;
        private long offsetFinal = 0;
        private readonly IConsumerConnection _connection;

        protected EventListenerBase(IConsumerConnection connection, TopicSettings topicSettings, ILogger<EventListenerBase> logger)
        {
            _topicSettings = topicSettings;
            _logger = logger;
            _connection = connection;
            _consumer = connection.GetListenerConsumer(topicSettings);
        }

        public void Commit()
        {
            try
            {
                _consumer.Commit(_topicPartitionOffsets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Dictionary<string, string> RetrieveNextBatch()
        {
            _connection.StartReadingStatus();
            _topicPartitionOffsets = new List<TopicPartitionOffset>();

            var response = RetrieveBatchMessages()
                .AsParallel()
                .ToList().ToDictionary(x=> x.Key, x=> x.Value);

            return response;
        }

        private IEnumerable<CdcModel> RetrieveBatchMessages()
        {
            DateTime executionDate = DateTime.Now;
            _currentReadCount = 0;

            do
            {
                offsetInicial = 0;
                if (TryRetrieve(out string key, out string message))
                {
                    _currentReadCount++;
                    yield return new CdcModel { Key = key, Value = message };
                }
            } while (CanReadNext(executionDate));
            _logger.LogInformation($"Total messages read: {_currentReadCount}");
            _logger.LogInformation("-------------------------------------");
            _logger.LogInformation($"initial offset: {offsetInicial}, final offset: {offsetFinal}");
            _logger.LogInformation("-------------------------------------");
        }

        private bool TryRetrieve(out string key, out string message)
        {
            key = null;
            message = null;
            try
            {
                var consumeResult = _consumer.Consume(millisecondsTimeout: _topicSettings.ConsumeTimeoutMs);
                if (!string.IsNullOrEmpty(consumeResult?.Message?.Value))
                {
                    _topicPartitionOffsets.Add(consumeResult.TopicPartitionOffset);
                    if (offsetInicial == 0)
                        offsetInicial = consumeResult.Offset.Value;
                    offsetFinal = consumeResult.Offset.Value;
                    key = consumeResult?.Key;
                    message = consumeResult?.Message?.Value;
                    return true;
                }
            }
            catch (ConsumeException e)
            {
                _logger.LogWarning(e, "Error al intentar obtener el evento de kafka.");
            }

            return false;
        }

        private bool CanReadNext(DateTime executionDate) =>
            executionDate.AddMilliseconds(_topicSettings.RecopilationTimeMs) > DateTime.Now
            &&
            _currentReadCount < _topicSettings.MaxItemsDequeue;
    }
}
