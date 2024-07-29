Console.WriteLine("Starting server on port 8080");
Console.WriteLine("Server address: http://localhost:8080");
HTTPServer.HTTPServer server = new HTTPServer.HTTPServer(8080);
await server.Start();