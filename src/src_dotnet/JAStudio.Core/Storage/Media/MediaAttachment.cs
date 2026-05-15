using System.Text.Json.Serialization;

namespace JAStudio.Core.Storage.Media;

public abstract class MediaAttachment
{
   public string? OriginalFileName { get; init; }
   [JsonIgnore] public string FilePath { get; init; } = string.Empty;
}

public class AudioAttachment : MediaAttachment;

public class ImageAttachment : MediaAttachment;
