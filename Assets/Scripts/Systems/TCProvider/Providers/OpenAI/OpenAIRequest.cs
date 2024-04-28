using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// A class to track a single request to the LLM AIs provided by OpenAI.
/// </summary>
public class OpenAIRequest {

    public Action<OpenAIRequest> OnFinish;

    public string ErrorMessage { get; private set; }

    public bool RequestInProgress { get; private set; }

    private TCOperation linkedOperation;

    // Cached in case we need to retry the request
    private CancellationToken lastUsedCancelToken;

    public OpenAIRequest(TCOperation textCompletionOperation) {
        linkedOperation = textCompletionOperation;
    }

    private Role GetRoleFromId(string roleId) {
        switch(roleId) {
            default:
            case TCOperation.SYSTEM_KEY:
                return Role.System;
            case TCOperation.USER_KEY:
                return Role.User;
            case TCOperation.ASSISTANT_KEY:
                return Role.Assistant;
        }
    }

    public void PerformRequest(OpenAIClient clientAPI, CancellationToken cancelToken) {
        if(linkedOperation == null) {
            Debug.LogError("OperationInProgress is null. Cannot perform request.");
            return;
        }

        linkedOperation.ClearResponse();
        ErrorMessage = string.Empty;

        // Grab the input sata from the linked operation
        Dictionary<string, string> operationInput = linkedOperation.GetAllOperationInput();
        List<Message> messages = new List<Message>();

        // Populate messages array from linkedOperation
        foreach(KeyValuePair<string, string> item in operationInput) {
            messages.Add(new Message(GetRoleFromId(item.Key), item.Value));
        }

        // Create ChatRequest from linkedOperation
        ChatRequest newChatRequest = new ChatRequest(messages, linkedOperation.ModelID);

        // Begin the request
        try {
            clientAPI.ChatEndpoint.StreamCompletionAsync(newChatRequest, ReceivedResponse, cancelToken);
            linkedOperation.RequestWasSent();

        } catch(Exception ex) {
            ErrorMessage = $"Type: {ex.GetType()}\nMessage: {ex.Message}\nInnerException: {ex.InnerException}\nStackTrace:\n{ex.StackTrace}";
        }
    }

    private void ReceivedResponse(ChatResponse response) {

        // Check if this is a partial response
        if(string.IsNullOrEmpty(response.FirstChoice.FinishReason) || response.FirstChoice.Message == null) {
            linkedOperation.AppendResponse(response.FirstChoice.Delta.ToString());
            if(!string.IsNullOrEmpty(response.FirstChoice.Delta)) {
                linkedOperation.AppendResponseLog($"Received Delta [{response.FirstChoice.Delta}]");
            }
            return;
        }

        // Record Usage info if we have it.
        if(response.Usage != null && response.Usage.PromptTokens.HasValue) {
            linkedOperation.RequestTokenCount = response.Usage.PromptTokens.Value;
        }
        if(response.Usage != null && response.Usage.PromptTokens.HasValue) {
            linkedOperation.ResponseTokenCount = response.Usage.CompletionTokens.Value;
        }

        // Otherwise this should be the completed response
        OnStreamFinished(response.FirstChoice);
    }

    private void OnStreamFinished(Choice choice) {
        if(ErrorMessage.Length != 0) {
            linkedOperation.SetErrorMessage(ErrorMessage);
        } else {
            linkedOperation.SetFinalResponse(choice.Message);
        }

        RequestInProgress = false;
        
        OnFinish?.Invoke(this);
    }
}
