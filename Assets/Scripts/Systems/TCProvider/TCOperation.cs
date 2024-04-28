using System;
using System.Collections.Generic;
using System.Text;
using TiktokenSharp;

/// <summary>
/// This class aims to completely define a single Text-Completion operation.
/// Including all inputs, outputs and metadata about the request.
/// This is deliberately LLM agnostic.
/// </summary>
public class TCOperation : IDebugInfo {

    public const string SYSTEM_KEY = "system";
    public const string USER_KEY = "user";
    public const string ASSISTANT_KEY = "assistant";

    #region EVENTS

    public Action<TCOperation> OnRequestStarted;
    public Action<TCOperation> OnRequestComplete;

    public Action<TCOperation> OnResponseStarted;
    public Action<TCOperation> OnResponseUpdated;
    public Action<TCOperation> OnResponseComplete;

    public void RequestWasStarted() {
        RequestStarted = DateTime.UtcNow;
        OnRequestStarted?.Invoke(this);
    }

    public void RequestWasSent() {
        RequestSent = DateTime.UtcNow;
        OnRequestComplete?.Invoke(this);
    }

    #endregion

    #region INPUTS

    /// <summary>
    /// Arbitrary ID for debugging.
    /// </summary>
    public Guid OperationGUID { get; private set; } = Guid.NewGuid();
    public string OperationID => OperationGUID.ToString("N").Substring(0,8).ToUpperInvariant().Insert(4,"-");

    // AI Cosmos Studios defines a Text-Completion operation as having the following inputs:

    /// <summary>
    /// A system message describing how the LLM should interpret or characterize the operation.
    /// </summary>
    public string Role { get; set; } = "You are a helpful assistant.";

    /// <summary>
    /// (Optional) A series of system messages that describe how to perform the desired operation.
    /// </summary>
    public List<string> Instructions { get; set; } = new List<string>();

    /// <summary>
    /// (Optional) A series of system messages that contain the contextual information required/useful context for the operation.
    /// </summary>
    public List<string> Context { get; set; } = new List<string>();

    /// <summary>
    /// (Optional) A series of user and assistant messages that contain relevant prior Operations.
    /// </summary>
    public Dictionary<string, string> History { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// A user message containing the current operation prompt.
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    /// This function will return a boolean value indicating whether the current inputs are valid.
    /// If the inputs are not valid, the result string will contain a message describing the validation failures.
    /// </summary>
    public bool ValidateInputs(out string result) {
        result = string.Empty;

        if(string.IsNullOrEmpty(Role)) {
            result += "Role is required. ";
        }

        if(string.IsNullOrEmpty(Prompt)) {
            result += "Prompt is required. ";
        }

        result = result.Trim();
        return result.Length == 0;
    }

    /// <summary>
    /// Returns the inputs.
    /// </summary>
    public Dictionary<string, string> GetAllOperationInput() {
        // Populate messages array from linkedOperation
        Dictionary<string, string> messages = new Dictionary<string, string> {
            { SYSTEM_KEY, Role }
        };
        // Add any instructions 
        if(Instructions != null && Instructions.Count > 0) {
            for(int i = 0; i < Instructions.Count; i++) {
                messages.Add(SYSTEM_KEY, Instructions[i]);
            }
        }
        // Add any context
        if(Context != null && Context.Count > 0) {
            for(int i = 0; i < Context.Count; i++) {
                messages.Add(SYSTEM_KEY, Context[i]);
            }
        }
        // Add any history
        if(History != null && History.Count > 0) {
            foreach(var item in History) {
                messages.Add(item.Key, item.Value);
            }
        }
        // Add the prompt
        messages.Add(USER_KEY, Prompt);

        // Cache the inputs as a json string
        LastGeneratedInputJSON = MessagesToJson(messages);

        return messages;
    }

    private string MessagesToJson(Dictionary<string, string> messages) {
        if(messages == null || messages.Count == 0) {
            return string.Empty;
        }
        // Start Json
        StringBuilder sb = new StringBuilder("{");

        // Add messages
        foreach(var message in messages) {
            sb.AppendLine($"\"{message.Key}\": \"{message.Value}\",");
        }
        // Remove trailing comma
        sb.Remove(sb.Length - 1, 1);
        
        // End json
        sb.Append("}");
        return sb.ToString();
    }

    public string LastGeneratedInputJSON { get; private set; }

    #endregion

    #region CONFIG

    /// <summary>
    /// Used to specifiy if there is a preference in which TCProvider completes this operation.
    /// </summary>
    public Type PreferredProvider = null;

    /// <summary>
    /// ModelProfile contains the desired properties of the LLM model that will perform a TCOperation.
    /// </summary>
    public TCModelProfile ModelProfile = new TCModelProfile();

    /// <summary>
    /// This is the actualised ID for the LLM model used, based on the ModelProfile specified.
    /// </summary>
    public string ModelID = string.Empty;

    #endregion

    #region OUTPUTS

    public string FinalResponse { get; private set; }
    public string ErrorMessage { get; private set; }

    public StringBuilder ResponseStream = new StringBuilder();
    public StringBuilder ResponseLog = new StringBuilder();

    public void ClearResponse() {
        FinalResponse = string.Empty;
        ResponseStream.Clear();
    }

    public void AppendResponse(string responseDelta) {
        bool wasEmpty = ResponseStream.Length == 0;

        ResponseStream.Append(responseDelta);
        
        // Check if this is the first response
        if(wasEmpty) {
            ResponseStarted = DateTime.UtcNow;
            OnResponseStarted?.Invoke(this);
        }

        // Call response updated after checking if we should call response started
        OnResponseUpdated?.Invoke(this);
    }

    public void AppendResponseLog(string newLog) {
        ResponseLog.AppendLine(newLog);
    }

    public void SetFinalResponse(string response) {
        FinalResponse = response;
        ResponseComplete();
    }

    public void SetErrorMessage(string errorMessage) {
        ErrorMessage = errorMessage;
        ResponseComplete();
    }

    private void ResponseComplete() {

        // TODO: Error handling and automatic retry

        ResponseReceived = DateTime.UtcNow;

        // If we have not been given any usage info, we calculate it here
        if(RequestTokenCount == 0 && ResponseTokenCount == 0 && !string.IsNullOrEmpty(ModelID)) {
            // Get the encoding for the model
            TikToken tikToken = TikToken.EncodingForModel(ModelID);

            // Use Tiktoken to count the tokens
            RequestTokenCount = tikToken.Encode(LastGeneratedInputJSON).Count;
            ResponseTokenCount = tikToken.Encode(FinalResponse).Count;
        }

        OnResponseComplete?.Invoke(this);
    }

    /// <summary>
    /// This function will return a boolean value indicating whether the final response is valid.
    /// To be considered valid the final response must be populated and there must be no error messages.
    /// </summary>
    public bool ValidateResponse() {
        return FinalResponse.Length > 0 && ErrorMessage.Length == 0;
    }

    #endregion

    #region METADATA

    public int RequestTokenCount;
    public int ResponseTokenCount;

    public DateTime RequestStarted;
    public DateTime RequestSent;
    public DateTime ResponseStarted;
    public DateTime ResponseReceived;

    public TimeSpan RequestDuration => RequestSent - RequestStarted;
    public TimeSpan WaitDuration => ResponseStarted - RequestSent;
    public TimeSpan ResponseDuration => ResponseReceived - ResponseStarted;
    public TimeSpan TotalDuration => ResponseReceived - RequestStarted;

    #endregion

    /// <summary>
    /// Returns all avalible information regarding this class in a human readable ordering and format.
    /// </summary>
    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}Text-Completion Operation [{OperationID}]");

        sb.AppendLine($"{prefix}*Metadata*");
        sb.AppendLine($"{prefix}\tRequestTokenCount [{RequestTokenCount}]");
        sb.AppendLine($"{prefix}\tResponseTokenCount [{ResponseTokenCount}]");
        sb.AppendLine($"{prefix}\tTotal Tokens [{RequestTokenCount + ResponseTokenCount}]");

        sb.AppendLine($"{prefix}\tRequest Started: {RequestStarted}");
        sb.AppendLine($"{prefix}\tRequest Sent: {RequestSent}");
        
        sb.AppendLine($"{prefix}\tResponse Started: {ResponseStarted}");
        sb.AppendLine($"{prefix}\tResponse Received: {ResponseReceived}");

        sb.AppendLine($"{prefix}\tRequest Duration: {RequestDuration.GetPrettyPrint()}");
        sb.AppendLine($"{prefix}\tWait Duration: {WaitDuration.GetPrettyPrint()}");
        sb.AppendLine($"{prefix}\tResponse Duration: {ResponseDuration.GetPrettyPrint()}");
        sb.AppendLine($"{prefix}\tTotal Duration: {TotalDuration.GetPrettyPrint()}");

        sb.AppendLine($"{prefix}*Config*");
        ModelProfile.GetDebugInfo(sb, $"{prefix}\t");
        sb.AppendLine($"{prefix}\tModelID [{ModelID}]");

        sb.AppendLine($"{prefix}*Inputs*");
        sb.AppendLine($"{prefix}\tRole [{Role}]");
        sb.AppendLine($"{prefix}\tInstructions:");
        foreach(var instruction in Instructions) {
            sb.AppendLine($"{prefix}\t\t[{instruction}]");
        }
        sb.AppendLine($"{prefix}\tContext:");
        foreach(var context in Context) {
            sb.AppendLine($"{prefix}\t\t[{context}]");
        }
        sb.AppendLine($"{prefix}\tHistory:");
        foreach(var history in History) {
            sb.AppendLine($"{prefix}\t\t[{history.Key}]: [{history.Value}]");
        }
        sb.AppendLine($"{prefix}\tPrompt [{Prompt}]");

        sb.AppendLine($"{prefix}*Outputs*");
        sb.AppendLine($"{prefix}\tFinal Response [{FinalResponse}]");
        sb.AppendLine($"{prefix}\tError Message [{ErrorMessage}]");
        sb.AppendLine($"{prefix}\tFull Response Log:");
        string[] logLines = ResponseLog.ToString().Split(new char[] { '\n', '\r'});
        foreach(string logLine in logLines) {
            if(string.IsNullOrEmpty(logLine)) {
                continue;
            }
            sb.AppendLine($"{prefix}\t\t{logLine.Trim()}");
        }
    }
}
