using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveService.Subscription
{
    public class Keys
    {
        public static readonly string Key = "37a8bf5363104bdbb6352b14e51fe8cf";
        public static readonly string Endpoint = "https://teamcompressior.cognitiveservices.azure.com/";

        public ComputerVisionClient Config()
        {
            string apiKey = Keys.Key;
            string endpoint = Keys.Endpoint;
            var computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = endpoint
            };
            return computerVisionClient;
        }
    }
}
