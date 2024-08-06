using Server = HTTPServer;

Server.LocalIpAddressResolver localIpAddressResolver = new Server.LocalIpAddressResolver();
var ipAddress = localIpAddressResolver.GetIPAddress();
int port = 8080;

Console.WriteLine($"Starting server on: http://{ipAddress}:{port}");
Server.HTTPServer server = new Server.HTTPServer(ipAddress, port);
await server.Start();