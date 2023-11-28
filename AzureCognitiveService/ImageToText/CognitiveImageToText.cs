using AzureCognitiveService.Subscription;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveService.ImageToText
{
    public class CognitiveImageToText
    {
        public async Task<List<KeyValue>> ImageToText(string imagePath)
        {
            var computerVisionClient = new Keys().Config();
            var imgToText = await ReadTextFromImage(computerVisionClient, imagePath);
            List<KeyValue> keyValues = new List<KeyValue>();
            foreach (var x in imgToText)
            {
                var model = new KeyValue()
                {
                    key = x.Key,
                    value = x.Value
                };
                keyValues.Add(model);
            }
            return keyValues;
        }
        private async Task<Dictionary<string, string>> ReadTextFromImage(ComputerVisionClient client, string imagePath)
        {
            var ocrResult = await client.RecognizeTextInStreamAsync(GetImageStream(imagePath), TextRecognitionMode.Handwritten);
            Thread.Sleep(1000);
            var extractedText = await CongitiveApiCall(ocrResult.OperationLocation, Keys.Key);
            return extractedText;
        }
        private async Task<Dictionary<string, string>> CongitiveApiCall(string uri, string subsKey)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", subsKey);
            IRestResponse response = await client.ExecuteAsync(request);
            var result = JsonConvert.DeserializeObject<Root>(response.Content);
            var lines = result.recognitionResult.lines.Select(x => x.text).ToList();
            bool benificary = false;
            for (int i = 0; i < lines.Count() - 1; i++)
            {
                if (lines[i].Contains("Part 2"))
                {
                    benificary = true;
                }
                if (lines[i].Contains(":") && !lines[i].Contains("Part"))
                {
                    if (!lines[i + 1].Contains(":"))
                    {
                        if (i + 1 < lines.Count() && lines[i + 1].Length <= 15)
                            dict[benificary ? "Beneficary " + lines[i].Replace(":", "") : "Decessed " + lines[i].Replace(":", "")] = lines[i + 1];
                    }
                }
            }
            return dict;
        }
        private Stream GetImageStream(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file not found.", imagePath);
            }
            FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
        public async Task<string> GetRawTextFromImg(string imagePath)
        {
            var computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Keys.Key))
            {
                Endpoint = Keys.Endpoint
            };
            string text = await ReadTextFromImageRaw(computerVisionClient, imagePath);
            return text;
        }
        private async Task<string> ReadTextFromImageRaw(ComputerVisionClient client, string imagePath)
        {
            var ocrResult = await client.RecognizeTextInStreamAsync(GetImageStream(imagePath), TextRecognitionMode.Handwritten);
            Thread.Sleep(1000);
            var extractedText = await CongitiveApiCallRaw(ocrResult.OperationLocation, Keys.Key);
            return extractedText;
        }
        private async Task<string> CongitiveApiCallRaw(string uri, string subsKey)
        {
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", subsKey);
            IRestResponse response = await client.ExecuteAsync(request);
            var result = JsonConvert.DeserializeObject<Root>(response.Content);
            var lines = result.recognitionResult.lines.Select(x => x.text).ToList();
            string text = string.Join(" ", lines);
            return text;

        }
        

    }
}
