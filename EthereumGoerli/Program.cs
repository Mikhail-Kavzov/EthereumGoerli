using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using EthereumGoerli.Models;
using System.Net.Http.Json;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;

internal class Program
{
    public static IConfiguration Configuration { get; private set; }

    public static HttpClient InfuraServer { get; private set; }
    
    public static EthereumRequest EthereumRequestPolling { get; private set; }

    static  Program()
    {
        var exeDir = Directory.GetCurrentDirectory();
        var directory = Directory.GetParent(exeDir).Parent?.Parent?.FullName;
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

        var ethParams = new Dictionary<string, string>
        {
            { "address", Configuration.GetConnectionString("WalletPublicKey") }
        };

        EthereumRequestPolling = new EthereumRequest("eth_getLogs",
            new Dictionary<string, string>[] {ethParams});
    }

    private static async void EthereumTransactionRequest(object? state)
    {
        var response = await InfuraServer.PostAsJsonAsync(string.Empty, EthereumRequestPolling);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine($"status: {response.StatusCode}");
        }
    }

    private static void Main(string[] args)
    {
        using (var timer = new Timer(EthereumTransactionRequest, null, 0, 10000))
        {
            Console.ReadLine();
        }
    }
}