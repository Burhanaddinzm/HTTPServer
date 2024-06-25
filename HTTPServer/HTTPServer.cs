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
    public const string NAME = "Test HTTP Server v0.1";

    private int _port;
    private bool _running = false;

    private TcpListener _listener;

    public HTTPServer(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    public void Start()
    {
        Thread serverThread = new Thread(new ThreadStart(Run));
        serverThread.Start();
    }

    private void Run()
    {
        _running = true;
        _listener.Start();

        while (_running)
        {
            Console.WriteLine("Waiting for connection...");

            TcpClient client = _listener.AcceptTcpClient();

            Console.WriteLine("Client connected");

            HandleClient(client);

            client.Close();
        }

        _running = false;

        _listener.Stop();
    }

    private void HandleClient(TcpClient client)
    {
        StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
        StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };

        try
        {
            StringBuilder requestBuilder = new StringBuilder();
            string line;

            while ((line = reader.ReadLine()) != null && line != "")
            {
                requestBuilder.Append(line + "\n");
            }

            string requestMsg = requestBuilder.ToString();
            Debug.WriteLine("Request: \n" + requestMsg);

            Request req = Request.GetRequest(requestMsg);

            Response resp = Response.From(req);

            resp.Post(writer);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error handling client: " + ex.Message);

            Response errorResponse = Response.MakeInternalServerError();
            errorResponse.Post(writer);
        }
        finally
        {
            reader.Close();
            writer.Close();
        }
    }

}
