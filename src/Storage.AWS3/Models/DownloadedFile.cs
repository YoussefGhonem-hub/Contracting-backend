namespace Storage.AWS3.Models;

public class DownloadedFile
{
    public byte[] Contents { get; }
    public string FileName { get; }
    public string ContentType { get; }
    public long ContentLength { get; }

    public DownloadedFile(byte[] contents, string contentType, string fileName)
    {
        Contents = contents;
        FileName = fileName;
        ContentType = contentType;
    }

}