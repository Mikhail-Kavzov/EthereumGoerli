using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EthereumGoerli.Models
{

    public class EthereumRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; }
        [JsonPropertyName("method")]
        public string Method { get; }
        [JsonPropertyName("params")]
        public Dictionary<string,string>[] Params { get; }
        [JsonPropertyName("jsonrpc")]
        public string JsonRPC { get; }

        public EthereumRequest(int id, string jsonRPC, string method,
            Dictionary<string, string>[] methodParams)
        {
            Id = id;
            Method = method;
            Params = methodParams;
            JsonRPC = jsonRPC;
        }

        public EthereumRequest(string method, Dictionary<string, string>[] methodParams)
            : this(1, "2.0", method, methodParams)
        { }
    }
}
