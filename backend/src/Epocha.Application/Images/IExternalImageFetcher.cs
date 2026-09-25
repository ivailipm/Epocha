namespace Epocha.Application.Images;

public record ExternalImage(byte[] Content, string ContentType);

/// <summary>Fetches an image from a museum's own site. Implemented in Infrastructure.</summary>
public interface IExternalImageFetcher
{
    Task<ExternalImage?> FetchAsync(string url, CancellationToken cancellationToken);
}
