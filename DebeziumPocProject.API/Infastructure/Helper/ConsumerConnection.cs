using Confluent.Kafka;
using DebeziumPocProject.API.Infastructure.Settings;
using Microsoft.Extensions.Options;

namespace DebeziumPocProject.API.Infastructure.Helper
{
    public class ConsumerConnection : IConsumerConnection
    {
        private readonly ILogger<ConsumerConnection> _logger;
        private TopicSettings _topicSettings;
        public bool RowsReadedSuccessfully { get { return _rowsReadedSuccessfully; } }
        private bool _rowsReadedSuccessfully { get; set; }
        private readonly ConsumerConfig _consumerConfig;

        public ConsumerConnection(IOptions<TopicSettings> settings, ILogger<ConsumerConnection> logger)
        {
            _logger = logger;
            _topicSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _consumerConfig = new ConsumerConfig()
            {
                BootstrapServers = _topicSettings.BrokerList,
                GroupId = _topicSettings.ConsumerGroup,
                StatisticsIntervalMs = _topicSettings.StatisticsIntervalMs,
                SessionTimeoutMs = _topicSettings.SessionTimeoutMs,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = _topicSettings.AutoCommit,
                EnablePartitionEof = true,
                MaxPollIntervalMs = _topicSettings.MaxPollIntervalMs,
                AutoCommitIntervalMs = 5000,
                EnableAutoOffsetStore = true,
            };
        }

        public IConsumer<string, string> GetListenerConsumer(TopicSettings config)
        {
            _topicSettings = config;

            var instance = CreateConsumer();
            instance.Subscribe(config.TopicName);

            return instance;
        }

        public void StartReadingStatus() { _rowsReadedSuccessfully = true; }
        public bool GetReadingStatus() { return _rowsReadedSuccessfully; }

        private IConsumer<string, string> CreateConsumer()
        {
            return new ConsumerBuilder<string, string>(_consumerConfig)
                 .SetErrorHandler((_, error) =>
                 {
                     _rowsReadedSuccessfully = false;
                     _logger.LogError("Error on kafka consumer: code [{code}] - Error Information: {@reason}", error.Reason, error.Code);
                 })
                 .SetPartitionsAssignedHandler((_, partitions) =>
                 {
                     _logger.LogInformation("Assigned partitions: {@partitions}", partitions.Select(p => p.Partition.Value));
                 })
                 .SetPartitionsRevokedHandler((_, partitions) =>
                 {
                     _logger.LogInformation("Revoked partitions: {@partitions}", partitions.Select(p => p.Partition.Value));
                 })
                 .Build();
        }
    }
}
