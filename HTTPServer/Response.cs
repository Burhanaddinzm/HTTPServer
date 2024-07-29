namespace HTTPServer;

public class Response
{
    private byte[]? data = null;
    private string status;
    private string mime;

    private Response(string status, string mime, byte[] data)
    {
        this.data = data;
        this.status = status;
        this.mime = mime;
    }

    public static Response From(Request? request)
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
            case ".svg":
                return "image/svg+xml";
            case ".pdf":
                return "application/pdf";
            case ".xml":
                return "application/xml";
            case ".json":
                return "application/json";
            case ".mp4":
                return "video/mp4";
            case ".mp3":
                return "audio/mpeg";
            case ".zip":
                return "application/zip";
            case ".txt":
                return "text/plain";
            case ".csv":
                return "text/csv";
            case ".doc":
            case ".docx":
                return "application/msword";
            case ".xls":
            case ".xlsx":
                return "application/vnd.ms-excel";
            case ".ppt":
            case ".pptx":
                return "application/vnd.ms-powerpoint";
            case ".odt":
                return "application/vnd.oasis.opendocument.text";
            case ".ods":
                return "application/vnd.oasis.opendocument.spreadsheet";
            case ".odp":
                return "application/vnd.oasis.opendocument.presentation";
            case ".wav":
                return "audio/wav";
            case ".ogg":
                return "audio/ogg";
            case ".mpg":
            case ".mpeg":
                return "video/mpeg";
            case ".avi":
                return "video/x-msvideo";
            case ".tar":
                return "application/x-tar";
            case ".gz":
                return "application/gzip";
            case ".7z":
                return "application/x-7z-compressed";
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

    public async Task PostAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync($"{HTTPServer.VERSION} {status}");
        await writer.WriteLineAsync($"Server: {HTTPServer.NAME}");
        await writer.WriteLineAsync($"Content-Type: {mime}");
        await writer.WriteLineAsync($"Content-Length: {data.Length}");
        await writer.WriteLineAsync("Connection: close");
        await writer.WriteLineAsync();
        await writer.FlushAsync();

        await writer.BaseStream.WriteAsync(data, 0, data.Length);
        await writer.BaseStream.FlushAsync();
    }
}
