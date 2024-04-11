#define OPEN_AI_ENABLED
#define ANTHROPIC_ENABLED
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// This class is made to manage the various sources that can provide a Large Language Model Text Completion operation.
/// </summary>
public class TCProviderManager : MonoSingleton<TCProviderManager>, IDebugInfo {

    List<ITCProvider> KnownTCProviders = new List<ITCProvider>();

    protected override void OnInstanceInitialised() {

        // Testing request
        TCOperation debugOperation = new TCOperation();
        debugOperation.Prompt = "Hello, Claude";
        debugOperation.RequestWasStarted();
        debugOperation.OnResponseComplete += (TCOperation operation) => {
            StringBuilder sb = new StringBuilder();
            operation.GetDebugInfo(sb);
            Debug.Log(sb.ToString());
        };

#if OPEN_AI_ENABLED
        // Add Open AI provider
        OpenAIProvider openIAProvider = new OpenAIProvider();
        openIAProvider.SetOnDestroyCancelToken(destroyCancellationToken);
        KnownTCProviders.Add(openIAProvider);
#endif

#if ANTHROPIC_ENABLED
        // Add Anthropic provider
        AnthropicProvider anthropicProvider = new AnthropicProvider();
        anthropicProvider.SetOnDestroyCancelToken(destroyCancellationToken);
        KnownTCProviders.Add(anthropicProvider);

        // For debugging
        anthropicProvider.PerformTextCompletion(debugOperation);
#endif
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
