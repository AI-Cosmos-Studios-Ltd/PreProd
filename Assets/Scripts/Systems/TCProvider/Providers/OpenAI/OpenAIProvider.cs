using System.Text;
using System.Threading;

public class OpenAIProvider : ITCProvider {

    private static string PROVIDER_ID = "OpenAI";
    public string ProviderId => PROVIDER_ID;

    private CancellationToken cancelToken;

    public void OnDestroy() {
        // Cancel any requests in progress

        // Teardown the loaded API
    }

    public void SetOnDestroyCancelToken(CancellationToken cancelToken) {
        this.cancelToken = cancelToken;
    }

    public void PerformTextCompletion(TCOperation operation) {
        
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}OpenAIProvider");
        sb.AppendLine($"{prefix}Provider ID [{ProviderId}]");
    }
}
