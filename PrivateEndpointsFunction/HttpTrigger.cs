using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace PrivateEndpointsFunction
{
    public class HttpTrigger
    {
        private readonly ILogger _logger;

        public HttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTrigger>();
        }

        [Function("PrivateEndTester")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            HttpClient client = new HttpClient();

            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions();

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);            

            string url = req.Query["url"];
            int port = int.Parse(req.Query["port"]);

            PingReply reply = ping.Send(url);

            var ipAddress = reply.Address;

            try
            {
                await socket.ConnectAsync(ipAddress, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            bool connected = socket.Connected;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString($"Ping Reply Result: {connected}");

            return response;
        }
    }
}
