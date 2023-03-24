namespace DisasterPR.Backends;

public class DefaultBackend : IBackend
{
    public Stream? GetHttpStream(Uri uri)
    {
        var http = new HttpClient();
        var result = http.GetStreamAsync(uri).Result;
        return result;
    }

    public void GetHttpStreamAsync(Uri uri, Action<Stream> callback, Action<Exception>? onError = null)
    {
        _ = Task.Run(() =>
        {
            try
            {
                callback(GetHttpStream(uri)!);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        });
    }
}