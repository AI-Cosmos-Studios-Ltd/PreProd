using System.Threading;

public interface ITCProvider : IDebugInfo {
    public string ProviderId { get; }
    public void PerformTextCompletion(TCOperation operation);

    public string GetModelIdFromProfile(TCModelProfile profile);

    public void SetOnDestroyCancelToken(CancellationToken cancelToken);
    public void OnDestroy();
}
