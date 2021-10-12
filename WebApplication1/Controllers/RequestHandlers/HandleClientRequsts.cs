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
            consumerManager.AddCancellationToken(properties.CommunicationType, source);
        }
        /// <summary>
        /// writes decoded messages according to the request properties to kafka
        /// </summary>
        /// <param name="decodedMessageProps"></param>
        /// <param name="communicationsDict"></param>
        /// <param name="IcdInitilizer"></param>
        public void ProduceDecodedMessageTask(DecodedMessagePropertiesDto decodedMessageProps, CommuncationsIcdDict communicationsDict, IcdDataInitialiazer IcdInitilizer, ProducerConfig config )
        {
            CancellationTokenSource source;
            source = new CancellationTokenSource();
            AddProducerRequst(decodedMessageProps, source);
            Task.Run(async () =>
            {
                while (!source.Token.IsCancellationRequested)
                {
                    string topic = decodedMessageProps.CommunicationType + "-" + decodedMessageProps.DataDirection;
                    string communicationName = communicationsDict.GetCommunicationIcdName(decodedMessageProps.CommunicationType, decodedMessageProps.DataDirection); //Get Icd File Name
                    string finalPath = Path.Combine(communicationsDict.IcdFilesPath, communicationName + ".txt");
                    DecodedFrameCreator rndDecoded = new DecodedFrameCreator(finalPath);
                    DecodedFrameDto decodedFrame = await rndDecoded.CreateRandomFrameTask(config, topic, source.Token); //creates random frames
                    KafkaProducer producer = KafkaProducer.GetInstance(config);
                    await producer.WriteToKafkaAsync(topic, decodedFrame, source.Token); //writing data to kafka
                    await Task.Delay(decodedMessageProps.TransmissionRate);
                }
                ClientActiveRequest.Remove(decodedMessageProps);
            }, source.Token);
        }
        public bool StopRequest(string communcationName)
        {
            if (this.TokenDictionary.ContainsKey(communcationName))
            {
                this.TokenDictionary[communcationName].Cancel();
                this.TokenDictionary.Remove(communcationName);
                KafkaConsumerManager consumerManager = KafkaConsumerManager.GetInstance();
                consumerManager.CancelKafkaConsumer(communcationName); //cancelling consumer on the topic
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
                if (currentDataBase.CommunicationsIcdDict.ContainsKey(communicationName) && !CheckIfCommunicationActive(communicationName))
                    return true;
                return false;
            }
            return false;
        }
    }
}
