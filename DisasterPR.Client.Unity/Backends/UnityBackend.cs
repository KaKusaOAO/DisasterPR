using DisasterPR.Backends;
using UnityEngine;
using UnityEngine.Networking;

namespace DisasterPR.Client.Unity.Backends;

public class UnityBackend : MonoBehaviour, IBackend
{
    private List<Func<bool>> _tickables = new();

    void Update()
    {
        var removal = _tickables.Where(func => func()).ToList();
        _tickables.RemoveAll(f => removal.Contains(f));
    }

    public Stream? GetHttpStream(Uri uri)
    {
        throw new NotImplementedException();
    }

    public void GetHttpStreamAsync(Uri uri, Action<Stream> callback, Action<Exception>? onError = null)
    {
        var request = UnityWebRequest.Get(uri);
        request.SendWebRequest();
        
        _tickables.Add(() =>
        {
            if (!request.isDone) return false;
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(new Exception(request.error));
                return true;
            }

            callback(new MemoryStream(request.downloadHandler.data));
            return true;
        });
    }
}