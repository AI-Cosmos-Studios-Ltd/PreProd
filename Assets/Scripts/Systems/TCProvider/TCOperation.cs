using System;
using System.Collections.Generic;
using System.Text;
using Utilities.WebRequestRest;

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

    public void RequestIsComplete() {
        RequestCompleted = DateTime.UtcNow;
        OnRequestComplete?.Invoke(this);
    }

    #endregion

    #region INPUTS

    /// <summary>
    /// Arbitrary ID for debugging.
    /// </summary>
    public Guid OperationGUID { get; private set; } = Guid.NewGuid();
    public string OperationID => OperationGUID.ToString("N").Substring(0,6);

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
    /// (Optional) A series of system messages that contain the contextual information required for the operation.
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
        result = "";

        if(string.IsNullOrEmpty(Role)) {
            result += "Role is required. ";
        }

        if(string.IsNullOrEmpty(Prompt)) {
            result += "Prompt is required. ";
        }

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

        return messages;
    }

    #endregion

    #region CONFIG

    // TODO: Change this to be a generic enum, make ITCProvider enforce the creation of a function to convert the enum to a concrete model.
    public string Model = "claude-3-opus-20240229";

    // TODO: Figure out if we need to make this more generic. Check if Anthropic use a simmilar throttling system to OpenAI.
    public int MaxTokens = 1024;

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

        ResponseCompleted = DateTime.UtcNow;
        OnResponseComplete?.Invoke(this);
    }

    public bool ValidateResponse() {
        return FinalResponse.Length > 0 && ErrorMessage.Length == 0;
    }

    #endregion

    #region METADATA

    public DateTime RequestStarted;
    public DateTime RequestCompleted;
    public DateTime ResponseStarted;
    public DateTime ResponseCompleted;

    public TimeSpan RequestDuration => RequestCompleted - RequestStarted;
    public TimeSpan WaitDuration => ResponseStarted - RequestCompleted;
    public TimeSpan ResponseDuration => ResponseCompleted - ResponseStarted;
    public TimeSpan TotalDuration => ResponseCompleted - RequestStarted;

    #endregion

    /// <summary>
    /// Returns all avalible information regarding this class in a user friendly ordering and format.
    /// </summary>
    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}Text-Completion Operation [{OperationID}]");

        sb.AppendLine($"{prefix}*Metadata*");
        sb.AppendLine(prefix + $"\tRequest Started: {RequestStarted}");
        sb.AppendLine(prefix + $"\tRequest Completed: {RequestCompleted}");
        
        sb.AppendLine(prefix + $"\tResponse Started: {ResponseStarted}");
        sb.AppendLine(prefix + $"\tResponse Completed: {ResponseCompleted}");

        sb.AppendLine(prefix + $"\tRequest Duration: {RequestDuration}");
        sb.AppendLine(prefix + $"\tWait Duration: {WaitDuration}");
        sb.AppendLine(prefix + $"\tResponse Duration: {ResponseDuration}");
        sb.AppendLine(prefix + $"\tTotal Duration: {TotalDuration}");

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
        sb.AppendLine($"{prefix}\t\t[{ResponseLog}]");

    }
}
