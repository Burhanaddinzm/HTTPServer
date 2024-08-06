using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTPServer;

public class HTTPServer
{
    public const string MSG_DIR = "/root/msg/";
    public const string WEB_DIR = "/root/web";
    public const string VERSION = "HTTP/1.1";
    public const string NAME = "Test HTTP Server v0.25";

    private bool _running = false;

    private TcpListener _listener;

    public HTTPServer(IPAddress ipAddress, int port)
    {
        _listener = new TcpListener(ipAddress, port);
    }

    public async Task Start()
    {
        await Task.Run(() => RunAsync());
    }

    private async Task RunAsync()
    {
        _running = true;
        _listener.Start();

        while (_running)
        {
            Console.WriteLine("Waiting for connection...");

            TcpClient client = await _listener.AcceptTcpClientAsync();
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;

            Console.WriteLine("Client connected");

            await HandleClientAsync(client);
            client.Close();
        }

        _running = false;

        _listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
        StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };

        try
        {
            StringBuilder requestBuilder = new StringBuilder();
            string? line;

            while (true)
            {
                line = await ReadLineWithTimeoutAsync(reader, TimeSpan.FromSeconds(1));
                if (line == null) break;
                if (line == "") break;
                if (line == string.Empty) break;
                requestBuilder.Append(line + "\n");
            }
            //while ((line = reader.ReadLine()) != null && line != "" && line != string.Empty)
            //{
            //    requestBuilder.Append(line + "\n");
            //}

            string requestMsg = requestBuilder.ToString();
            Debug.WriteLine("Request: \n" + requestMsg);

            Request? req = Request.GetRequest(requestMsg);

            Response resp = Response.From(req);

            await resp.PostAsync(writer);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error handling client: " + ex.Message);

            Response errorResponse = Response.MakeInternalServerError();
            await errorResponse.PostAsync(writer);
        }
        finally
        {
            reader.Close();
            writer.Close();
        }
    }

    private async Task<string?> ReadLineWithTimeoutAsync(StreamReader reader, TimeSpan timeout)
    {
        var readTask = reader.ReadLineAsync();
        var delayTask = Task.Delay(timeout);

        var completedTask = await Task.WhenAny(readTask, delayTask);

        if (completedTask == delayTask)
        {
            return null;
        }

        return await readTask;
    }
}
