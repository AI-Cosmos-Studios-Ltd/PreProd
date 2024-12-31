using OpenAI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class OpenAIProvider : ITCProvider {

    private static string PROVIDER_ID = "OpenAI";
    public string ProviderId => PROVIDER_ID;

    private OpenAIClient clientAPI;

    private List<OpenAIRequest> requestsInProgress = new List<OpenAIRequest>();

    private CancellationToken cancelToken;
    private bool hasLoadedAPI = false;

    private void EnsureAPIIsLoaded() {
        if(hasLoadedAPI) {
            return;
        }

        // Fetch the config asset
        OpenAIConfiguration config = Resources.Load<OpenAIConfiguration>("OpenAIConfiguration");

        if(config == null) {
            config = Resources.LoadAll<OpenAIConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);
        }

        // Load the API if it is not already loaded
        clientAPI = new OpenAIClient(config);

        hasLoadedAPI = true;
    }

    public void OnDestroy() {
        // Cancel any requests in progress

        // Teardown the loaded API
        if(hasLoadedAPI) {
            clientAPI = null;
        }
    }

    public void SetOnDestroyCancelToken(CancellationToken cancelToken) {
        this.cancelToken = cancelToken;
    }

    public void PerformTextCompletion(TCOperation operation) {
        EnsureAPIIsLoaded();

        // Check we have a model ID, if not get one.
        if(string.IsNullOrEmpty(operation.ModelID)) {
            operation.SetLLMModelUsed(GetModelIdFromProfile(operation.ModelProfile));
        }

        if(string.IsNullOrEmpty(operation.ModelID)) {
            Debug.LogError("Unable to find a LLM which matches TCModelProfile. Cannot perform request.");
            return;
        }

        OpenAIRequest newRequest = new OpenAIRequest(operation);
        newRequest.OnFinish += RequestFinished;

        requestsInProgress.Add(newRequest);

        newRequest.PerformRequest(clientAPI, cancelToken);
    }

    private void RequestFinished(OpenAIRequest finishedRequest) {
        if(requestsInProgress.Contains(finishedRequest)) {
            requestsInProgress.Remove(finishedRequest);
        }
    }

    public string GetModelIdFromProfile(TCModelProfile profile) {
        // If the desired max tokens is over what GPT3 can handle then we need to use GPT-4
        if(profile.Need_MinTokenLimit > OpenAIModelData.GPT3_TURBO_16K.ContextSize) {
            return OpenAIModelData.GPT4_TURBO.ModelId;
        }

        // If the sophistication is important then we should use GPT-4
        if(profile.Want_Sophistication > 0.5f && profile.Want_Sophistication > profile.Want_Cost) {
            return OpenAIModelData.GPT4_TURBO.ModelId;
        }

        // Otherwise it's GPT3 for general use (because it's cheaper).
        return OpenAIModelData.GPT3_TURBO_16K.ModelId;
    }

    public void ListAvalibleModels() {
        EnsureAPIIsLoaded();
        ListAvalibleModelsAsync();
    }

    private async void ListAvalibleModelsAsync() {
        var models = await clientAPI.ModelsEndpoint.GetModelsAsync();

        // TODO: Sort?

        StringBuilder sb = new StringBuilder();
        foreach(var model in models) {
            sb.AppendLine(model.ToString());
        }
        Debug.Log(sb);
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}OpenAIProvider");
        sb.AppendLine($"{prefix}Provider ID [{ProviderId}]");
    }
}
