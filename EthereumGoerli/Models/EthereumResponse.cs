using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EthereumGoerli.Models
{
    public class EthereumResponse<T>
    {
        [JsonPropertyName("id")]
        public int Id { get; set;  }
        [JsonPropertyName("result")]
        public T Result { get; set; }
        [JsonPropertyName("jsonrpc")]
        public string JsonRPC { get; set; }
    }
}
