using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidCom
{
    public class MessageConfig
    {
        public string MessageTopic { get; set; } = "kidcom/message";
        public string CloseTopic { get; set; } = "kidcom/close";
        public string Server { get; set; } = "10.10.10.1";
        public string CilentId { get; set; } = "kid1";
    }

    public class MessageHandler
    {
        public MessageHandler(Action<string> onMessage, Action onClose)
        {
            OnMessage = onMessage;
            OnClose = onClose;
        }

        public Action<string> OnMessage { get; internal set; }
        public Action OnClose { get; internal set; }
    }


    public class BackgroundMessageHandler
    {
        private MessageConfig _config;
        private MessageHandler _messageHandler;

        public void Start(MessageConfig config, MessageHandler handler)
        {
            _config = config;
            _messageHandler = handler;


            var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(_config.CilentId)
                        .WithTcpServer(_config.Server)
                        //.WithTls()
                        .Build())
                    .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();

            mqttClient.UseApplicationMessageReceivedHandler((args) =>
            {
                var topic = args.ApplicationMessage.Topic;
                if (topic.Equals(_config.MessageTopic)) {
                    var message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                    _messageHandler?.OnMessage?.Invoke(message);
                }
                else if (topic.Equals(_config.CloseTopic))
                {
                    _messageHandler?.OnClose?.Invoke();
                }
            });

            Task.Run(async () =>
            {
                await mqttClient.SubscribeAsync(
                    new TopicFilterBuilder().WithTopic(_config.MessageTopic).Build(), 
                    new TopicFilterBuilder().WithTopic(_config.CloseTopic).Build()
                );
                await mqttClient.StartAsync(options);
            });
        }
    }
}
