using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace RMitra.Api.Response;

[Serializable]
[DataContract]
public class ApiResponseMessage
{
    [JsonProperty("Success")]
    [DataMember]
    public bool Success { get; set; }

    [JsonProperty("Status_Code")]
    [DataMember]
    public int Status_Code { get; set; }

    [JsonProperty("Internel_Status_Code")]
    [DataMember]
    public int Internel_Status_Code { get; set; }

    [JsonProperty("Message")]
    [DataMember]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("Method_Name")]
    [DataMember]
    public string Method_Name { get; set; } = string.Empty;

    [JsonProperty("Data")]
    [DataMember]
    public object? Data { get; set; }

    [JsonProperty("Model_State")]
    [DataMember]
    public ModelStateDictionary? Model_State { get; set; }
}

[Serializable]
[DataContract]
public class ApiErrorResponseMessage
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public int Status_Code { get; set; }

    [DataMember]
    public string Message { get; set; } = string.Empty;

    [DataMember]
    public string Error_Message { get; set; } = string.Empty;

    [DataMember]
    public int Error_Code { get; set; }

    [DataMember]
    public string Method_Name { get; set; } = string.Empty;

    [DataMember]
    public int Internel_Status_Code { get; set; }

    [DataMember]
    public object? Data { get; set; }
}
