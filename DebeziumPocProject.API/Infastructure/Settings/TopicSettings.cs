namespace DebeziumPocProject.API.Infastructure.Settings
{
    public class TopicSettings
    {
        public string BrokerList { get; set; }
        public string ConsumerGroup { get; set; }
        public int StatisticsIntervalMs { get; set; }
        public int SessionTimeoutMs { get; set; }
        public bool AutoCommit { get; set; }
        public int MaxPollIntervalMs { get; set; }
        public string TopicName { get; set; }
        public int RecopilationTimeMs { get; set; }
        public int MaxItemsDequeue { get; set; }
        public int ConsumeTimeoutMs { get; set; }
    }
}
