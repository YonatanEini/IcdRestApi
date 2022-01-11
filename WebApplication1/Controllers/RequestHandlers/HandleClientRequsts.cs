using Confluent.Kafka;
using HandleIcdLibrary;
using IcdFilesRestApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApplication1;
using HandleKafkaLib;
using IcdFilesRestApi.Controllers.RequestsHandlers;
using HandleKafkaLibrary;
using HandleKafkaLibrary.ClientConsumers;
using System.Net;
using HandleKafkaLibrary.CosumersProperties;

namespace DecodedIcd.Controllers.RequestHandlers
{
    public class HandleClientRequests
    {
        public List<DecodedMessagePropertiesDto> ClientActiveRequest { get; set; }
        public Dictionary<string, CancellationTokenSource> TokenDictionary { get; set; }
        private HandleClientRequests()
        {
            this.ClientActiveRequest = new List<DecodedMessagePropertiesDto>();
            this.TokenDictionary = new Dictionary<string, CancellationTokenSource>();
        }
        private static HandleClientRequests _instance = null;
        public static HandleClientRequests GetInstance()
        {
            _instance = _instance ?? new HandleClientRequests();
            return _instance;
        }
        public void AddProducerRequst(DecodedMessagePropertiesDto properties, CancellationTokenSource source)
        {
            this.TokenDictionary.Add(properties.CommunicationType, source);
            this.ClientActiveRequest.Add(properties);
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            consumerManager.AddTopicCancellationToken(properties.CommunicationType, source);
        }
        /// <summary>
        /// writes decoded messages according to the request properties to kafka
        /// </summary>
        /// <param name="decodedMessageProps"></param>
        /// <param name="communicationsDict"></param>
        /// <param name="IcdInitilizer"></param>
        public async Task ProduceDecodedMessageAsync(DecodedMessagePropertiesDto decodedMessageProps, 
            CommuncationsIcdDict communicationsDict, IcdDataInitialiazer IcdInitilizer, ProducerConfig config )
        {
            CancellationTokenSource source;
            source = new CancellationTokenSource();
            AddProducerRequst(decodedMessageProps, source);
            while (!source.Token.IsCancellationRequested)
            {
                string topic = decodedMessageProps.CommunicationType + "-" + decodedMessageProps.DataDirection;
                //search for the Icd File Name
                string communicationName = communicationsDict.GetCommunicationIcdName(decodedMessageProps.CommunicationType,
                    decodedMessageProps.DataDirection); 
                string finalPath = Path.Combine(communicationsDict.IcdFilesPath, communicationName + ".txt");
                DecodedFrameCreator rndDecoded = new DecodedFrameCreator(finalPath);
                //creates random frames
                DecodedFrameDto decodedFrame = await rndDecoded.CreateRandomFrameAsyncTask(source.Token); 
                KafkaProducer producer = KafkaProducer.GetInstance(config);
                //writing data to kafka
                _ = Task.Factory.StartNew(() => producer.WriteToKafkaAsync(topic, decodedFrame, source.Token));  
                //client transmission rate delay
                await Task.Delay(decodedMessageProps.TransmissionRate);
            }
            ClientActiveRequest.Remove(decodedMessageProps);
        }
        public bool StopRequest(string communcationName)
        {
            if (this.TokenDictionary.ContainsKey(communcationName))
            {
                this.TokenDictionary[communcationName].Cancel();
                this.TokenDictionary.Remove(communcationName);
                return true;
            }
            return false;
        }
        public void CancelAllRequests()
        {
            foreach (var TokenDictionaryItem in TokenDictionary)
            {
                TokenDictionaryItem.Value.Cancel();
            }
            this.ClientActiveRequest.Clear();
        }
        /// <summary>
        /// checks if there is active decode request with communcationName
        /// </summary>
        /// <param name="communcationName"></param>
        /// <returns></returns>
        public bool CheckIfCommunicationActive(string communcationName)
        {
            foreach (var name in this.ClientActiveRequest)
            {
                if (name.CommunicationType.Equals(communcationName))
                    return true;
            }
            return false;
        }
        public bool CheckClientMessageProperties(CommuncationsIcdDict currentDataBase, DecodedMessagePropertiesDto clientProps)
        {
            EnumCommunicationType dataDirection = clientProps.DataDirection;
            string communicationName = clientProps.CommunicationType;
            if (dataDirection.Equals(EnumCommunicationType.Uplink) || dataDirection.Equals(EnumCommunicationType.Downlink))
            {
                if (currentDataBase.CommunicationsIcdDict.ContainsKey(communicationName) && 
                    !CheckIfCommunicationActive(communicationName))
                    return true;
                return false;
            }
            return false;
        }
        public HttpStatusCode SearchClientProperties(ClientPropertiesBase properties)
        {
            HttpStatusCode responseStatusCode = HttpStatusCode.NotAcceptable;
            KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
            foreach (var topic in properties.ConsumerTopic)
            {
                if (consumerManager.TopicClientsDict.ContainsKey(topic))
                {
                    List<ClientDataReceiverBase> topicKafkaListeners = consumerManager.TopicClientsDict[topic];
                    foreach (var client in topicKafkaListeners)
                    {
                        if (client.CompareProperties(properties))
                        {
                            consumerManager.TopicClientsDict[topic].Remove(client);
                            responseStatusCode = HttpStatusCode.Created;
                            break;
                        }
                    }
                }
            }
            return responseStatusCode;
        }
    }
}
