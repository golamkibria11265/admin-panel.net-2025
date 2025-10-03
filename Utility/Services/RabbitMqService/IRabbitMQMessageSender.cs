using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Utility.Services.RabbitMqService
{
    public interface IRabbitMQMessageSender
    {
        void SendMessage(Object message, string queueName);
    }
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private IConfiguration _iConfiguration;
        public RabbitMQMessageSender(IConfiguration iConfiguration)
        {
            _iConfiguration = iConfiguration;
        }

        public void SendMessage(object message, string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = _iConfiguration.GetValue<string>("RabbitMQ:HostName"),
                Password = _iConfiguration.GetValue<string>("RabbitMQ:Password"),
                UserName = _iConfiguration.GetValue<string>("RabbitMQ:UserName")
            };
            var _connection = factory.CreateConnection();

            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName, false, false, false, null);
            var jsonObject = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonObject);
            channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
    }
}
