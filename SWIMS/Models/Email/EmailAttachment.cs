using System.IO;

namespace SWIMS.Models.Email;

public sealed class EmailAttachment
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }

    public static EmailAttachment FromFile(string path, string contentType) =>
        new EmailAttachment
        {
            FileName = Path.GetFileName(path),
            ContentType = contentType,
            Content = File.ReadAllBytes(path)
        };
}
