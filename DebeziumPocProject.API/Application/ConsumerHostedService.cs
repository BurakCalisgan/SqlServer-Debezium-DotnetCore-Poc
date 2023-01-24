using DebeziumPocProject.API.Infastructure.Interfaces;
using Newtonsoft.Json;

namespace DebeziumPocProject.API.Application
{
    public class ConsumerHostedService : BackgroundService
    {
        private Timer _timer;
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IEventListener _eventListener;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IEventListener eventListener)
        {
            _eventListener = eventListener;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var topicResponse = _eventListener.RetrieveNextBatch();
                foreach (var message in topicResponse)
                {
                    //_logger.LogInformation($"Message: {JsonConvert.SerializeObject(message)}");
                    //mongo save()

                    _logger.LogInformation($"Message: {JsonConvert.SerializeObject(message)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading data");
            }
        }
    }
}
