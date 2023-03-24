namespace DisasterPR.Backends;

public interface IBackend
{
    public Stream? GetHttpStream(Uri uri);
    public void GetHttpStreamAsync(Uri uri, Action<Stream> callback, Action<Exception>? onError = null);
}