/// <summary>
/// A class to define the desired properties of the LLM model that will perform a TCOperation.
/// Properties are deliberately generic so it can be applicable by the widest range of LLM Providers.
/// TCProviders will use this to determine which LLM model should be used to perform the Text-Completion.
/// </summary>
public class TCModelProfile : IDebugInfo {

    // The required capabilities are used to determine if a model is capable of performing the task.
    // If these cannot be fulfilled the TCProvider will not perform the TCOperation and will return an error.
    #region REQUIRED_CAPABILITIES

    /// <summary>
    /// The minimum acceptable token limit for the model.
    /// </summary>
    public readonly int Need_MinTokenLimit = 1024;

    #endregion

    // Desireable qualities are used to determine which LLM model is best
    // suited to the task when multiple models can fulfil the requirements.
    #region DESIREABLE_QUALITIES

    /// <summary>
    /// The importance of the model having a fast response time.
    /// A value of 1.0 is the most important, 0.0 is the least important.
    /// </summary>
    public readonly float Want_Speed = 0f;

    /// <summary>
    /// The importance of the model having a low request/response cost.
    /// A value of 1.0 is the most important, 0.0 is the least important.
    /// </summary>
    public readonly float Want_Cost = 0f;

    /// <summary>
    /// The importance of the model having the ability to generate the most sophisticated responses.
    /// Higher values will encourage the use of the newest and most advanced models.
    /// A value of 1.0 is the most important, 0.0 is the least important.
    /// </summary>
    public readonly float Want_Sophistication = 0f;

    /// <summary>
    /// The token limit that the model should ideally have.
    /// </summary>
    public readonly int Want_TokenLimit = 2048;

    #endregion

    #region DEBUG_INFO
    public void GetDebugInfo(System.Text.StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TCModelProfile");
        sb.AppendLine($"{prefix}\tMinTokenLimit [{Need_MinTokenLimit}]");
        sb.AppendLine($"{prefix}\tIdealTokenLimit [{Want_TokenLimit}]");
        sb.AppendLine($"{prefix}\tSophistication [{Want_Sophistication}]");
        sb.AppendLine($"{prefix}\tSpeed [{Want_Speed}]");
        sb.AppendLine($"{prefix}\tCost [{Want_Cost}]");
    }
    #endregion
}
