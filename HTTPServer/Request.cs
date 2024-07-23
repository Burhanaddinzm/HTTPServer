namespace HTTPServer;

public class Request
{
    public string Type { get; set; }
    public string URL { get; set; }
    public string Host { get; set; }
    public string Referer { get; set; }

    public Request(string type, string url, string host, string referer)
    {
        Type = type;
        URL = url.Replace("%20", " ");
        Host = host;
        Referer = referer;
    }

    public static Request GetRequest(string request)
    {
        if (string.IsNullOrEmpty(request)) return null;

        string[] lines = request.Split('\n');
        string[] requestLine = lines[0].Split(' ');

        if (requestLine.Length < 3) return null;

        string type = requestLine[0];
        string url = requestLine[1];
        string host = string.Empty;
        string referer = string.Empty;

        foreach (var line in lines)
        {
            if (line.StartsWith("Host:"))
            {
                host = line.Substring("Host:".Length).Trim();
            }
            else if (line.StartsWith("Referer:"))
            {
                referer = line.Substring("Referer:".Length).Trim();
            }
        }

        Console.WriteLine($"{type} {url} @ {host} \nReferer: {referer}");

        return new Request(type, url, host, referer);
    }
}
