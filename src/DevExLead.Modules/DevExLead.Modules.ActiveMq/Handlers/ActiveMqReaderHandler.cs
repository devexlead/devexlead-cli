using Apache.NMS;
using Apache.NMS.ActiveMQ;
using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.ActiveMq.Helpers;
using Spectre.Console;
using System.Text;
using System.Text.Json;

namespace DevExLead.Modules.ActiveMq.Handlers
{
    public class ActiveMqReaderHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var queueName = ParameterHelper.ReadStringParameter(options, "queue");

            // Environment selection using Spectre.Console
            var selectedEnvironment = ActiveMqHelper.SelectEnvironment();
            AnsiConsole.MarkupLine($"[blue]Selected environment: {selectedEnvironment}[/]");

            // Get connection details based on selected environment
            var (brokerUri, username, password) = ActiveMqHelper.GetConnectionDetails(selectedEnvironment);

            if (string.IsNullOrEmpty(brokerUri))
            {
                AnsiConsole.MarkupLine("[red]ActiveMQ connection string not found. Please configure ActiveMQ settings.[/]");
                return;
            }

            // Create connection factory
            IConnectionFactory factory = new ConnectionFactory(brokerUri);

            // Create a connection using credentials
            using (IConnection connection = factory.CreateConnection(username, password))
            {
                connection.Start();

                using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination destination = session.GetQueue(queueName);

                    // Use QueueBrowser to peek messages
                    using (IQueueBrowser messagesBrowser = session.CreateBrowser((IQueue)destination))
                    {
                        AnsiConsole.MarkupLine($"[cyan]Browsing messages in queue: {queueName} on {selectedEnvironment} environment[/]");

                        // Collect messages for JSON serialization
                        var messages = new List<object>();

                        foreach (IMessage message in messagesBrowser)
                        {
                            // Safely access properties that may throw NotImplementedException
                            DateTime? timestamp = null;
                            DateTime? deliveryTime = null;

                            try
                            {
                                timestamp = message.NMSTimestamp;
                            }
                            catch (NotImplementedException)
                            {
                                // Property not implemented, leave as null
                            }

                            try
                            {
                                deliveryTime = message.NMSDeliveryTime;
                            }
                            catch (NotImplementedException)
                            {
                                // Property not implemented, leave as null
                            }

                            var messageData = new
                            {
                                MessageId = message.NMSMessageId,
                                Timestamp = timestamp,
                                DeliveryMode = message.NMSDeliveryMode.ToString(),
                                Priority = message.NMSPriority.ToString(),
                                Redelivered = message.NMSRedelivered,
                                Type = message.NMSType,
                                CorrelationId = message.NMSCorrelationID,
                                DeliveryTime = deliveryTime,
                                TimeToLive = message.NMSTimeToLive.TotalMilliseconds,
                                Destination = message.NMSDestination?.ToString(),
                                ReplyTo = message.NMSReplyTo?.ToString(),
                                Body = (object)null,
                                BodyType = message.GetType().Name
                            };

                            if (message is ITextMessage textMessage)
                            {
                                messageData = messageData with { Body = textMessage.Text, BodyType = "TextMessage" };
                                AnsiConsole.MarkupLine($"[green][Peeked] Message: {textMessage.Text}[/]");
                            }
                            else if (message is IBytesMessage bytesMessage)
                            {
                                var buffer = new byte[bytesMessage.BodyLength];
                                bytesMessage.ReadBytes(buffer);
                                string body = Encoding.UTF8.GetString(buffer);
                                
                                messageData = messageData with { Body = body, BodyType = "BytesMessage" };

                                try
                                {
                                    Console.WriteLine(body);
                                }
                                catch (Exception ex)
                                {
                                    AnsiConsole.MarkupLine($"[red][Deserialize Error] {ex.Message}[/]");
                                }
                            }
                            else
                            {
                                messageData = messageData with { Body = "Non-text message", BodyType = message.GetType().Name };
                                AnsiConsole.MarkupLine("[yellow][Peeked] Non-text message received.[/]");
                            }

                            messages.Add(messageData);
                        }

                        // Serialize to JSON
                        var jsonOptions = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        string json = JsonSerializer.Serialize(new
                        {
                            QueueName = queueName,
                            Environment = selectedEnvironment,
                            BrowsedAt = DateTime.UtcNow,
                            MessageCount = messages.Count,
                            Messages = messages
                        }, jsonOptions);

                        AnsiConsole.MarkupLine($"[cyan]JSON Output:[/]");
                        Console.WriteLine(json);

                        var rootPath = Directory.GetCurrentDirectory();
                        var logFileName = $"activemq_browse_{queueName}_{selectedEnvironment}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        var logFilePath = Path.Combine(rootPath, logFileName);
                        await File.WriteAllTextAsync(logFilePath, json);
                        AnsiConsole.MarkupLine($"[green]JSON output saved to: {logFilePath}[/]");
                    }
                }
            }
        }
    }
}
