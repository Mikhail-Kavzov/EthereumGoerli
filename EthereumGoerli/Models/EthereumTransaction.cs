using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EthereumGoerli.Models
{
    public class EthereumTransaction
    {
        private const long WEI_TO_ETH_CONVERTER = 1000000000000000000;

        [JsonPropertyName("hash")]
        public string TransactionHash { get; set; }

        [JsonPropertyName("from")]
        public string Sender { get; set; }

        [JsonPropertyName("to")]
        public string Receiver { get; set; }

        [JsonPropertyName("value")]
        public string Sum { get; set; }

        public override string ToString()
        {
            var sumInEth = Convert.ToInt64(Sum, 16) / (double)WEI_TO_ETH_CONVERTER;
            return $"Sum: {sumInEth} eth" +
                $"Hash: {TransactionHash}  " +
                $" TokenAddress: {Sender} ";
        }
    }
}
