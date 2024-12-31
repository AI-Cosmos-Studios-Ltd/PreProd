using Claudia;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

public class AnthropicProvider : ITCProvider {

    private static string PROVIDER_ID = "Anthropic";
    public string ProviderId => PROVIDER_ID;

    private List<AnthropicRequest> requestsInProgress = new List<AnthropicRequest>();

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

        // Check we have a model ID, if not get one.
        if(string.IsNullOrEmpty(operation.ModelID)) {
            operation.SetLLMModelUsed(GetModelIdFromProfile(operation.ModelProfile));
        }

        if(operation.ModelID == string.Empty) {
            Debug.LogError("Unable to find a LLM which matches TCModelProfile. Cannot perform request.");
            return;
        }

        AnthropicRequest newRequest = new AnthropicRequest(operation);
        newRequest.OnFinish += RequestFinished;

        requestsInProgress.Add(newRequest);

        newRequest.PerformRequest(anthropicAPI, cancelToken);
    }

    private void RequestFinished(AnthropicRequest finishedRequest) {
        if(requestsInProgress.Contains(finishedRequest)) {
            requestsInProgress.Remove(finishedRequest);
        }
    }

    public string GetModelIdFromProfile(TCModelProfile profile) {
        // Currently we are only using one model.
        return "claude-3-opus-20240229";
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}AnthropicProvider");
        sb.AppendLine($"{prefix}Provider ID [{ProviderId}]");
        sb.AppendLine($"{prefix}# Requests In Progress [{requestsInProgress.Count}]");
    }
}
