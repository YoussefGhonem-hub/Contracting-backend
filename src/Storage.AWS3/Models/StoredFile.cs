namespace Storage.AWS3.Models;

public class StoredFile
{
    public string Key { get; set; } = null!;
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }

}