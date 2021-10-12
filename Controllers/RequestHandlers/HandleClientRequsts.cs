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

namespace DecodedIcd.Controllers.RequestHandlers
{
    public class HandleClientRequests
    {
        public List<DecodedMessagePropertiesDTO> ClientActiveRequest { get; set; }
        public Dictionary<string, CancellationTokenSource> TokenDictionary { get; set; }
        private HandleClientRequests()
        {
            this.ClientActiveRequest = new List<DecodedMessagePropertiesDTO>();
            this.TokenDictionary = new Dictionary<string, CancellationTokenSource>();
        }
        public static HandleClientRequests _instance = null;
        public static HandleClientRequests GetInstance()
        {
            _instance = _instance ?? new HandleClientRequests();
            return _instance;
        }
        public void AddRequst(DecodedMessagePropertiesDTO properties, CancellationTokenSource source)
        {
            this.TokenDictionary.Add(properties.CommunicationType, source);
            this.ClientActiveRequest.Add(properties);
        }
        /// <summary>
        /// writes decoded messages according to the request properties to a file
        /// </summary>
        /// <param name="decodedMessageProps"></param>
        /// <param name="communicationsDict"></param>
        /// <param name="IcdInitilizer"></param>
        public async void WritingDecodedMessage(DecodedMessagePropertiesDTO decodedMessageProps, CommuncationsIcdDict communicationsDict, IcdDataInitialiazer IcdInitilizer, ProducerConfig config )
        {
            CancellationTokenSource source;
            source = new CancellationTokenSource();
            AddRequst(decodedMessageProps, source);
            while (!source.Token.IsCancellationRequested)
            {
                string topic = decodedMessageProps.CommunicationType + "-" + decodedMessageProps.DataDirection;
                string communicationName = communicationsDict.GetCommunicationName(decodedMessageProps.CommunicationType, decodedMessageProps.DataDirection);
                string finalPath = Path.Combine(communicationsDict.IcdFilesPath, communicationName + ".txt");
                DecodedFrameCreator rndDecoded = new DecodedFrameCreator(finalPath);
                DecodedFrameDTO decodedFrame = await rndDecoded.CreateRandomFrameTask(config, topic);
                KafkaProducer producer = KafkaProducer.GetInstance(config);
                producer.Write(topic, decodedFrame); //writing data to kafka
                string dataForFile = JsonConvert.SerializeObject(decodedFrame); //writing data to a file (for checking only!)
                CommuncationsIcdDict.AppendToFile(IcdInitilizer.DestinationPath + decodedMessageProps.CommunicationType, dataForFile, IcdInitilizer.DestinationPath);
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
        public bool CheckClientMessageProperties(CommuncationsIcdDict currentDataBase, DecodedMessagePropertiesDTO clientProps)
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
