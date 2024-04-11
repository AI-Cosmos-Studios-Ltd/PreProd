using Claudia;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/// <summary>
/// A class to track a single request to the LLM AIs provided by Anthropic.
/// </summary>
public class AnthropicRequest {

    public string ErrorMessage { get; private set; }

    private TCOperation linkedOperation;

    private List<IMessageStreamEvent> rawResponse = new List<IMessageStreamEvent>();
    private CancellationToken lastUsedCancelToken;

    public AnthropicRequest(TCOperation textCompletionOperation) {
        linkedOperation = textCompletionOperation;
    }

    public void PerformRequest(Anthropic anthropicAPI, CancellationToken cancelToken) {
        if(linkedOperation == null) {
            UnityEngine.Debug.LogError("OperationInProgress is null. Cannot perform request.");
            return;
        }

        linkedOperation.ClearResponse();
        rawResponse.Clear();
        ErrorMessage = string.Empty;

        // Grab the input sata from the linked operation
        Dictionary<string, string> operationInput = linkedOperation.GetAllOperationInput();
        List<Message> messages = new List<Message>();
        StringBuilder systemString = new StringBuilder();

        // Populate messages array from linkedOperation
        foreach(KeyValuePair<string, string> item in operationInput) {
            // Claude handles 'system' prompts differently
            if(item.Key == TCOperation.SYSTEM_KEY) {
                if(systemString.Length != 0) {
                    systemString.AppendLine();
                }
                systemString.AppendLine(item.Value);
                continue;
            }

            messages.Add(new Message { Role = item.Key, Content = item.Value });
        }

        // Create MessageRequest from linkedOperation
        MessageRequest tcRequest = new MessageRequest() {
            Model = "claude-3-opus-20240229",
            MaxTokens = 1024,
            System = systemString.ToString(),
            Messages = messages.ToArray()
        };

        PerformRequestAsync(anthropicAPI, tcRequest, cancelToken);
    }

    private async void PerformRequestAsync(Anthropic anthropicAPI, MessageRequest tcRequest, CancellationToken cancelToken) {
        lastUsedCancelToken = cancelToken;

        try {
            IAsyncEnumerable<IMessageStreamEvent> stream = anthropicAPI.Messages.CreateStreamAsync(tcRequest, cancellationToken:cancelToken);

            linkedOperation.RequestIsComplete();

            await foreach(var messageStreamEvent in stream) {
                ProcessMessageStreamEvent(messageStreamEvent);
            }

        } catch(ClaudiaException ex) {
            // E.g.
            // 400 - InvalidRequestError 
            // [ClaudiaException.Name]
            // [ClaudiaException.Message]
            ErrorMessage = $"{(int)ex.Status} - {ex.Status}\n{ex.Name}\n{ex.Message}";
        }

        OnStreamFinished();
    }

    private void ProcessMessageStreamEvent(IMessageStreamEvent messageStreamEvent) {
        rawResponse.Add(messageStreamEvent);
        linkedOperation.AppendResponseLog(messageStreamEvent.ToString());

        switch(messageStreamEvent) {

            //case MessageStart mstart:
            //    break;

            //case ContentBlockStart cbstart:
            //    break;

            //case Claudia.Ping ping:
            //    break;

            case ContentBlockDelta cbd:
                linkedOperation.AppendResponse(cbd.Delta.Text);
                break;

            //case ContentBlockStop cbstop:
            //    break;

            //case MessageStop mstop:
            //    break;

            default:
                break;
        }
    }

    private void OnStreamFinished() {
        if(ErrorMessage.Length != 0) {
            linkedOperation.SetErrorMessage(ErrorMessage);
        } 
        else {
            linkedOperation.SetFinalResponse(linkedOperation.ResponseStream.ToString());
        }
    }
}
