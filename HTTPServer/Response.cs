using System.Net.Sockets;

namespace HTTPServer;

public class Response
{
    private byte[] data = null;
    private string status;
    private string mime;

    private Response(string status, string mime, byte[] data)
    {
        this.data = data;
        this.status = status;
        this.mime = mime;
    }

    public static Response From(Request request)
    {
        try
        {
            if (request == null)
            {
                return MakeNullRequest();
            }

            if (request.Type == "GET")
            {
                string file = Environment.CurrentDirectory + HTTPServer.WEB_DIR + request.URL;
                FileInfo fi = new FileInfo(file);

                if (fi.Exists && fi.Extension.Contains("."))
                {
                    return MakeFromFile(fi);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(fi + "/");
                    if (!di.Exists)
                    {
                        return MakePageNotFound();
                    }
                    FileInfo[] files = di.GetFiles();
                    foreach (FileInfo f in files)
                    {
                        string n = f.Name;
                        if (n.Contains("default.html")
                            || n.Contains("default.htm")
                            || n.Contains("index.htm")
                            || n.Contains("index.html"))
                        {
                            fi = f;
                            return MakeFromFile(f);
                        }
                    }
                }

                if (!fi.Exists)
                {
                    return MakePageNotFound();
                }
            }
            else
            {
                return MakeMethodNotAllowed();
            }

            return MakePageNotFound();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return MakeInternalServerError();
        }
    }

    private static Response MakeFromFile(FileInfo fi)
    {
        using (FileStream fs = fi.OpenRead())
        {
            using (BinaryReader reader = new BinaryReader(fs))
            {
                byte[] d = new byte[fs.Length];
                reader.Read(d, 0, d.Length);
                return new Response("200 OK", GetMimeType(fi.Extension), d);
            }
        }
    }

    private static string GetMimeType(string extension)
    {
        switch (extension.ToLower())
        {
            case ".htm":
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            default:
                return "application/octet-stream";
        }
    }

    private static Response MakeNullRequest()
    {
        return MakeErrorResponse("400 Bad Request", HTTPServer.MSG_DIR + "400.html");
    }

    private static Response MakePageNotFound()
    {
        return MakeErrorResponse("404 Not Found", HTTPServer.MSG_DIR + "404.html");
    }

    private static Response MakeMethodNotAllowed()
    {
        return MakeErrorResponse("405 Method Not Allowed", HTTPServer.MSG_DIR + "405.html");
    }

    public static Response MakeInternalServerError()
    {
        return MakeErrorResponse("500 Internal Server Error", HTTPServer.MSG_DIR + "500.html");
    }

    private static Response MakeErrorResponse(string status, string filePath)
    {
        FileInfo fi = new FileInfo(Environment.CurrentDirectory + filePath);
        using (FileStream fs = fi.OpenRead())
        {
            using (BinaryReader reader = new BinaryReader(fs))
            {
                byte[] d = new byte[fs.Length];
                reader.Read(d, 0, d.Length);
                return new Response(status, "text/html", d);
            }
        }
    }

    public void Post(StreamWriter writer)
    {
        writer.WriteLine($"{HTTPServer.VERSION} {status}");
        writer.WriteLine($"Server: {HTTPServer.NAME}");
        writer.WriteLine($"Content-Type: {mime}");
        writer.WriteLine($"Content-Length: {data.Length}");
        writer.WriteLine("Connection: close");
        writer.WriteLine();
        writer.Flush();

        writer.BaseStream.Write(data, 0, data.Length);
        writer.BaseStream.Flush();
    }
}
