using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace EthereumGoerli.Models
{
    public class EthereumBlock
    {
        [JsonPropertyName("transactions")]
        public List<EthereumTransaction> TransactionList { get; set; }
    }
}
