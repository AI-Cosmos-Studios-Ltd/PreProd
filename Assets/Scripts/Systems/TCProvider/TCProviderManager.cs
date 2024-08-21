#define OPEN_AI_ENABLED
//#define ANTHROPIC_ENABLED
using System.Collections.Generic;
using System.Text;
using TiktokenSharp;
using UnityEngine;

/// <summary>
/// This class is made to manage the various sources that can provide a Large Language Model Text Completion operation.
/// </summary>
public class TCProviderManager : MonoSingleton<TCProviderManager>, IDebugInfo {

    List<ITCProvider> KnownTCProviders = new List<ITCProvider>();

    protected override void OnInstanceInitialised() {

        // Set the folder for Tiktoken where it can find the model files
        TikToken.PBEFileDirectory = "Assets/Data/TikToken";

#if OPEN_AI_ENABLED
        // Add Open AI provider
        AddProvider(new OpenAIProvider());
#endif

#if ANTHROPIC_ENABLED
        // Add Anthropic provider
        AddProvider(new AnthropicProvider());
#endif
    }

    private void PerformTest() {
        // Create test request
        TCOperation testOperation = new TCOperation() {
            Prompt = "Hello, Assistant"
        };

        testOperation.OnResponseComplete += (TCOperation operation) => {
            StringBuilder sb = new StringBuilder();
            operation.GetDebugInfo(sb);
            Debug.Log(sb.ToString());
        };

        PerformTextCompletion(testOperation);
    }

    public void PerformTextCompletion(TCOperation operation) {
        if(operation == null) {
            return;
        }

        // Mark that the request has begun.
        operation.RequestWasStarted();

        // Check if the operation has a preferred provider
        if(operation.PreferredProvider != null) {
            foreach(var provider in KnownTCProviders) { 
                if(provider.GetType() == operation.PreferredProvider) {
                    provider.PerformTextCompletion(operation);
                    return;
                }
            }
        }

        // Otherwise use the first available provider
        if(KnownTCProviders.Count > 0) {
            KnownTCProviders[0].PerformTextCompletion(operation);
        }
    }

    private void AddProvider(ITCProvider newProvider) {
        // Add cancellation token
        newProvider.SetOnDestroyCancelToken(destroyCancellationToken);
        // Add to known providers
        KnownTCProviders.Add(newProvider);
    }

    protected override void OnInstanceDestroyed() {
        foreach(var provider in KnownTCProviders) {
            provider.OnDestroy();
        }
        KnownTCProviders.Clear();
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"TCProviderManager");
        foreach(var provider in KnownTCProviders) {
            provider.GetDebugInfo(sb, prefix + "  ");
        }
    }
}
