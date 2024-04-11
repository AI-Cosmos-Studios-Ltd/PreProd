using Claudia;
using System.Text;
using System.Threading;
using UnityEngine;

public class AnthropicProvider : ITCProvider {

    private static string PROVIDER_ID = "Anthropic";
    public string ProviderId => PROVIDER_ID;

    private Anthropic anthropicAPI;
    private CancellationToken cancelToken;

    private void EnsureAnthropicAPI() {
        if(anthropicAPI != null) {
            return;
        }

        AnthropicConfiguration config = Resources.Load<AnthropicConfiguration>("AnthropicConfiguration");

        if(config == null) {
            Debug.LogError("AnthropicConfiguration not found in Resources.");
            anthropicAPI = new Anthropic();
            return;
        }

        anthropicAPI = new Anthropic() {
            ApiKey = config.apiKey
        };
    }

    public void OnDestroy() {
        // Teardown the loaded API
        anthropicAPI.Dispose();
        anthropicAPI = null;
    }

    public void SetOnDestroyCancelToken(CancellationToken cancelToken) {
        this.cancelToken = cancelToken;
    }

    public void PerformTextCompletion(TCOperation operation) {
        EnsureAnthropicAPI();

        AnthropicRequest newRequest = new AnthropicRequest(operation);

        newRequest.PerformRequest(anthropicAPI, cancelToken);
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}AnthropicProvider");
        sb.AppendLine($"{prefix}Provider ID [{ProviderId}]");
    }
}
