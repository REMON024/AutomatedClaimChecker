using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveService.Model
{
    public class CognitiveModel
    {

    }
}


public class Line
{
    [JsonProperty("boundingBox")]
    public List<int> boundingBox { get; set; }
    [JsonProperty("text")]
    public string text { get; set; }
    [JsonProperty("words")]
    public List<Word> words { get; set; }
}
public class RecognitionResult
{
    [JsonProperty("lines")]
    public List<Line> lines { get; set; }
}
public class Root
{
    [JsonProperty("status")]
    public string status { get; set; }
    [JsonProperty("recognitionResult")]
    public RecognitionResult recognitionResult { get; set; }
}
public class Word
{
    [JsonProperty("boundingBox")]
    public List<int> boundingBox { get; set; }
    [JsonProperty("text")]
    public string text { get; set; }
    [JsonProperty("confidence")]
    public string confidence { get; set; }
}

public class KeyValue
{
    public string key { get; set; }
    public string value { get; set; }
}


