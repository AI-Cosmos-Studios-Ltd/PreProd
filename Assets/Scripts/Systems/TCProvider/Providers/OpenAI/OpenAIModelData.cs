public class OpenAIModelData {

    public static readonly OpenAIModelData GPT4_TURBO = new OpenAIModelData() {
        ModelId = "gpt-4-turbo-2024-04-09",
        ContextSize = 128000,
        InputCostPerMillionTokens = 10.0f,
        OutputCostPerMillionTokens = 30.0f,
        MaxTokensPerMin = 1500000,
        MaxRequestsPerMin = 10000
    };

    // NOTE: This model has been superceeded by GPT4_TURBO (It's cheaper, faster and has higher limits). I'm leaving this here so I don't forget.
    public static readonly OpenAIModelData GPT4 = new OpenAIModelData() {
        ModelId = "gpt-4-1106-preview",
        ContextSize = 8000,
        InputCostPerMillionTokens = 30.0f,
        OutputCostPerMillionTokens = 60.0f,
        MaxTokensPerMin = 300000,
        MaxRequestsPerMin = 10000
    };

    public static readonly OpenAIModelData GPT3_TURBO_16K = new OpenAIModelData() {
        ModelId = "gpt-3.5-turbo-16k-0613",
        ContextSize = 16000,
        InputCostPerMillionTokens = 0.50f,
        OutputCostPerMillionTokens = 1.50f,
        MaxTokensPerMin = 2000000,
        MaxRequestsPerMin = 10000
    };

    public string ModelId;
    public int ContextSize;
    public float InputCostPerMillionTokens;
    public float OutputCostPerMillionTokens;
    public int MaxTokensPerMin;
    public int MaxRequestsPerMin;
}