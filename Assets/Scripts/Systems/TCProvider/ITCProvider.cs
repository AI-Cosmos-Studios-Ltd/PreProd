using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface ITCProvider : IDebugInfo {
    public string ProviderId { get; }
    public void PerformTextCompletion(TCOperation operation);

    public void SetOnDestroyCancelToken(CancellationToken cancelToken);
    public void OnDestroy();
}
