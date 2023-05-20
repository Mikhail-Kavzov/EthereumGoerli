using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using EthereumGoerli.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal class Program
{
    public static IConfiguration Configuration { get; private set; }

    public static HttpClient InfuraServer { get; private set; }

    public static EthereumRequest<object>
        EthereumRequestLastBlockNumber
    { get; private set; }

    private static string _previousLastBlockNumber = string.Empty;


    static Program()
    {
        var exeDir = Directory.GetCurrentDirectory();
        var directory = Directory.GetParent(exeDir)!.Parent?.Parent?.FullName;
        var builder = new ConfigurationBuilder()
         .SetBasePath(directory)
         .AddJsonFile("config.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        var infuraUri = Configuration.GetConnectionString("InfuraURL");

        //add HttpClient
        InfuraServer = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(10),
            BaseAddress = new Uri(infuraUri),
        };

        //add authorization to Infura
        var userName = Configuration.GetConnectionString("InfuraPublicKey");
        var password = Configuration.GetConnectionString("InfuraPrivateKey");
        var authenticationString = userName + ":" + password;
        var authParams = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
        InfuraServer.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authParams);

        EthereumRequestLastBlockNumber = new EthereumRequest<object>
            ("eth_blockNumber",
            new object[] { });
    }

    private static async void EthereumPollingRequestAsync(object value)
    {
        var receiver = (string)value;
        var lastBlock = await GetLastBlockNumberAsync();
        var currentBlock = lastBlock;
        var blockNumberInt = Convert.ToInt32(currentBlock, 16);
        List<Task<string>> blockTasks = new();

        while (currentBlock != _previousLastBlockNumber)
        {
            var blockRequest = new EthereumRequest<object>
            ("eth_getBlockByNumber",
            new object[] { currentBlock, true });
            blockTasks.Add(EthereumPostRequestAsync(blockRequest));
            --blockNumberInt;
            currentBlock = "0x" + Convert.ToString(blockNumberInt, 16);
        }
        _previousLastBlockNumber = lastBlock;
        try
        {
            var blocks = await Task.WhenAll(blockTasks);
            DisplayTransactions(blocks, receiver);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    private static void DisplayTransactions(string[] blocks, string receiver)
    {
        foreach (var block in blocks)
        {
            var ethereumBlock = JsonSerializer.Deserialize<EthereumResponse<EthereumBlock>>(block);
            var transactions = ethereumBlock!.Result.TransactionList;
            var walletTransactions = transactions
            .Where(t => t.Receiver != null && t.Receiver == receiver);

            foreach (var transaction in walletTransactions)
            {
                Console.WriteLine(transaction);
            }
        }
    }

    private static async Task<string> EthereumPostRequestAsync<T>(T value)
    {
        var response = await InfuraServer.PostAsJsonAsync(string.Empty, value);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Response failed. Status {response.StatusCode}");
        }
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }

    private static async Task<string> GetLastBlockNumberAsync()
    {
        var response = await EthereumPostRequestAsync(EthereumRequestLastBlockNumber);
        var lastBlockNumber = JsonSerializer.Deserialize<EthereumResponse<string>>(response)!.Result;
        return lastBlockNumber;
    }

    private static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Enter receiver wallet: ");
            var receiver = Console.ReadLine();
            if (Regex.IsMatch(receiver, @"^0x[a-fA-F0-9]{40}$"))
            {
                receiver = receiver.ToLower();
                Console.WriteLine("Start listening transactions... " +
                    "Press any key to change receiver wallet");
                var lastBlockNumber = await GetLastBlockNumberAsync();
                _previousLastBlockNumber = lastBlockNumber;
                using (var timer = new Timer(EthereumPollingRequestAsync, receiver, 0, 10000))
                {
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Incorrect wallet");
            }
            Console.WriteLine("Would you like to exit from program? 1 - Yes, 2 - No");
            var key = Console.ReadKey();
            if (key.KeyChar == '1')
            {
                break;
            }
            Console.WriteLine();
        }
    }
}