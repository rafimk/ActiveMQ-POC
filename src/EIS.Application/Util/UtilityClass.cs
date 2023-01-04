using System;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using EIS.Application.Exceptions;
using System.Threading.Tasks;
using EIS.Domain.Entities;
using EIS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EIS.Application.Util
{
    public class UtilityClass
    {
        public static string GetLocalIpAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            string Hostname = null;
            Hostname = System.Environment.MachineName;

            IPHostEntry hostEntry = Dns.GetHostEntry(Hostname); 
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return null;
        }

        public static async Task ConsumeEventAsync(EisEvent eisEvent, string queueName, EventHandler registry, ILogger log)
        {
            IMessageProcessor messageProcessor = registry.GetMessageProcessor();

            if (messageProcessor == null)
            {
                log.LogError("No message handler found for the event ID {id} in queue {queue}", eisEvent.EventId, queueName);
                throw new EisMessageProcessException("No Message Processor found for the queue");
            }

            try 
            {
                log.LogError("Message with event {event} received", eisEvent);
            }
            catch (Exception e)
            {
                log.LogError("Processing of message with id {id} failed with error {er}", eisEvent.EventId, e.Message);
                throw new EisMessageProcessException($"Processing event ID => {eisEvent.EventId}", e);
            }
        }
    }
}
