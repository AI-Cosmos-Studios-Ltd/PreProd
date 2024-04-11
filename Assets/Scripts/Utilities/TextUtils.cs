using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiktokenSharp;
using UnityEngine;
using UnityEngine.Networking;

public class TextUtils {
    private const string NONE_RESPONSE = "none";

    public static string ConvertHexIntToAsciiString(string in_string) {
        try {
            int testValue = int.Parse(in_string);
            int intAscii = Convert.ToInt32(testValue.ToString(), 16);
            char resultChar = (char)intAscii;
            return resultChar.ToString();
        } catch(Exception e) {
            Debug.LogError(e.Message);
            return in_string;
        }
    }

    public static string CleanUnicodeJunk(string in_string) {
        // clean the leading junk which I've never identified? Capitalization unicode?
        in_string = in_string.Replace("\\\\00a0", "");

        string junkTag = "\\\\";
        while(in_string.Contains(junkTag)) {
            int index = in_string.IndexOf(junkTag);
            string hexString = in_string.Substring(index + 2, 4);
            string unicodeAscii = ConvertHexIntToAsciiString(hexString);
            in_string = in_string.Replace(junkTag + hexString, unicodeAscii);
        }
        return in_string;
    }

    public static string GenerateErrorString(UnityWebRequest webRequest) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"UnityWebRequest ERROR");
        sb.AppendLine($"Error [{webRequest.error}]");
        sb.AppendLine($"StatusCode [{webRequest.responseCode}]");
        sb.AppendLine($"URL [{webRequest.url}]");
        sb.AppendLine($"Method [{webRequest.method}]");
        sb.AppendLine($"URI [{webRequest.uri}]");
        sb.AppendLine($"result [{webRequest.result}]");
        Dictionary<string, string> headers = webRequest.GetResponseHeaders();
        if(headers != null && headers.Count > 0) {
            int count = 0;
            foreach(KeyValuePair<string, string> kvp in headers) {
                sb.AppendLine($"Response Header [{count}]: Key [{kvp.Key}] Value [{kvp.Value}]");
                count++;
            }
        }
        return sb.ToString();
    }

    
    public static string GenerateDebugString(ChatResponse result) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"OpenAI.Chat.ChatResponse ID [{result.Id}]");
        sb.AppendLine($"Created [{result.CreatedAtUnixTimeSeconds}]");
        sb.AppendLine($"Model [{result.Model}]");
        if(result.Usage != null) {
            sb.AppendLine($"Usage.PromptTokens [{result.Usage.PromptTokens}]");
            sb.AppendLine($"Usage.CompletionTokens [{result.Usage.CompletionTokens}]");
            sb.AppendLine($"Usage.TotalTokens [{result.Usage.TotalTokens}]");
        } else {
            sb.AppendLine($"Usage object was null.");
        }
        sb.AppendLine($"Object [{result.Object}]");
        sb.AppendLine($"Organization [{result.Organization}]");
        sb.AppendLine($"ProcessingTime [{result.ProcessingTime}]");
        sb.AppendLine($"RequestId [{result.RequestId}]");
        if(result.Choices != null && result.Choices.Count > 0) {
            foreach(Choice choice in result.Choices) {
                sb.AppendLine($"Choice Index [{choice.Index}]. Value [{choice.FinishReason}]. Delta [{choice.Delta}]. Message [{choice.Message}]");
            }
        }
        return sb.ToString();
    }

    public static string GenerateShortDebugString(ChatResponse result) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Model [{result.Model}]");
        if(result.Choices != null && result.Choices.Count > 0) {
            foreach(Choice choice in result.Choices) {
                sb.AppendLine($"Message [{choice.Message}]");
            }
        }
        sb.AppendLine($"ProcessingTime [{result.ProcessingTime}]");
        return sb.ToString();
    }

    public static async void LogModels(OpenAIAuthentication auth = null) {
        OpenAIClient api = new OpenAIClient(auth);
        var models = await api.ModelsEndpoint.GetModelsAsync();
        StringBuilder sb = new StringBuilder();
        foreach(OpenAI.Models.Model model in models) {
            sb.AppendLine($"Engine [{model.Id}]");
        }
        Debug.Log(sb.ToString());
    }

    public static void LogChat(List<Message> messages) {
        StringBuilder sb = new StringBuilder();
        foreach(Message message in messages) {
            sb.AppendLine($"{message.Role}:{message.Content}");
        }
        Debug.Log(sb.ToString());
    }

    public static string ToCSV<T>(IEnumerable<T> enumerator, string delimiter = ",") {
        if(enumerator == null) {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder();
        int maxCount = enumerator.Count();
        int count = 0;
        foreach(T item in enumerator) {
            if(item == null) {
                continue;
            }
            string toAdd = item.ToString();
            if(string.IsNullOrEmpty(toAdd)) {
                maxCount--;
                continue;
            }
            sb.Append(toAdd);
            if(count < maxCount - 1) {
                sb.Append(delimiter);
            }
            count++;
        }
        return sb.ToString();
    }

    public static string ToCSV<T>(T[] array, char delimiter) {
        return ToCSV(array, delimiter.ToString());
    }

    public static string ToCSV<T>(T[] array, string delimiter = ",") {
        if(array == null || array.Length == 0) {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder();
        for(int x = 0; x < array.Length; x++) {
            if(array[x] == null) {
                continue;
            }
            string stringToAdd = array[x].ToString();
            if(string.IsNullOrEmpty(stringToAdd)) {
                continue;
            }
            sb.Append(stringToAdd);
            if(x < array.Length - 1) {
                sb.Append(delimiter);
            }
        }
        return sb.ToString();
    }

    public static string ToCSV<T>(List<T> list, char delimiter){  
        return ToCSV(list, delimiter.ToString());
    }

    public static string ToCSV<T>(List<T> array, string delimiter = ",") {
        if(array == null || array.Count == 0) {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder();
        for(int x = 0; x < array.Count; x++) {
            if(array[x] == null) {
                continue;
            }
            string stringToAdd = array[x].ToString();
            if(string.IsNullOrEmpty(stringToAdd)) {
                continue;
            }
            sb.Append(stringToAdd);
            if(x < array.Count - 1) {
                sb.Append(delimiter);
            }
        }
        return sb.ToString();
    }

    public static string ConcatenateStructuredStorageEntries(Dictionary<List<int>, string> structuredStorage, string entryDelimiter = ". ") {
        if(structuredStorage == null || structuredStorage.Count == 0) {
            return NONE_RESPONSE;
        }
        StringBuilder sb = new StringBuilder();
        List<KeyValuePair<List<int>, string>> goalEntries = structuredStorage.ToList();

        goalEntries.Sort((x, y) => {
            // Null checks
            if(x.Key == null && y.Key == null) {
                return 0;
            } else if(x.Key == null) {
                return -1;
            } else if(y.Key == null) {
                return 1;
            }

            int minLength = Mathf.Min(x.Key.Count, y.Key.Count);

            // Check each index and see if any are different, smallest index wins
            for(int i = 0; i < minLength; i++) {
                if(x.Key[i] < y.Key[i]) {
                    return -1;
                } else if(x.Key[i] > y.Key[i]) {
                    return 1;
                }
            }

            // If we get here, all the indices are the same, so the shorter one wins
            return x.Key.Count == y.Key.Count? 0 : x.Key.Count < y.Key.Count ? -1 : 1;
        });

        for(int x = 0; x < goalEntries.Count; x++) {
            sb.Append($"{ToCSV(goalEntries[x].Key, '.')}. {goalEntries[x].Value}");
            if(x < goalEntries.Count - 1 && !goalEntries[x].Value.EndsWith(entryDelimiter)) {
                sb.Append(entryDelimiter);
            }
        }
        return sb.ToString();
    }

    private const string SINGLE_SECOND = "one second ago";
    private const string PLURAL_SECOND = " seconds ago";

    private const string SINGLE_MINUTE = "a minute ago";
    private const string PLURAL_MINUTE = " minutes ago";

    private const string SINGLE_HOUR = "an hour ago";
    private const string PLURAL_HOUR = " hours ago";

    private const string SINGLE_DAY = "yesterday";
    private const string PLURAL_DAY = " days ago";

    private const string SINGLE_MONTH = "one month ago";
    private const string PLURAL_MONTH = " months ago";

    private const string SINGLE_YEAR = "one year ago";
    private const string PLURAL_YEAR = " years ago";

    public static string PrettyPrintTimespan(DateTime start, DateTime end) {
        return PrettyPrintTimespan(end - start);
    }

    public static string PrettyPrintTimespan(TimeSpan ts) {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        double delta = Math.Abs(ts.TotalSeconds);

        if(delta < 1 * MINUTE)
            return ts.Seconds == 1 ? SINGLE_SECOND : $"{ts.Seconds}{PLURAL_SECOND}";

        if(delta < 2 * MINUTE)
            return SINGLE_MINUTE;

        if(delta < 45 * MINUTE)
            return $"{ts.Minutes}{PLURAL_MINUTE}";

        if(delta < 90 * MINUTE)
            return SINGLE_HOUR;

        if(delta < 24 * HOUR)
            return $"{ts.Hours}{PLURAL_HOUR}";

        if(delta < 48 * HOUR)
            return SINGLE_DAY;

        if(delta < 30 * DAY)
            return $"{ts.Days}{PLURAL_DAY}";

        if(delta < 12 * MONTH) {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return months <= 1 ? SINGLE_MONTH : $"{months}{PLURAL_MONTH}";
        } else {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? SINGLE_YEAR : $"{years}{PLURAL_YEAR}";
        }
    }


    public static TimeSpan GetTimeSpanFromPrettyPrint(string prettyPrintTimeString) {

        if(string.IsNullOrEmpty(prettyPrintTimeString)) {
            return TimeSpan.Zero;
        }

        if(prettyPrintTimeString.EndsWith(SINGLE_SECOND)) {
            return new TimeSpan(0, 0, 1);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_SECOND)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_SECOND, "").Trim(), out int seconds)) {
                return new TimeSpan(0, 0, seconds);
            } else {
                return new TimeSpan(0, 0, 1);
            }
        } 
        else if(prettyPrintTimeString.EndsWith(SINGLE_MINUTE)) {
            return new TimeSpan(0, 1, 0);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_MINUTE)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_MINUTE, "").Trim(), out int minutes)) {
                return new TimeSpan(0, minutes, 0);
            } else {
                return new TimeSpan(0, 1, 0);
            }
        } 
        else if(prettyPrintTimeString.EndsWith(SINGLE_HOUR)) {
            return new TimeSpan(1, 0, 0);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_HOUR)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_HOUR, "").Trim(), out int hours)) {
                return new TimeSpan(hours, 0, 0);
            } else {
                return new TimeSpan(1, 0, 0);
            }
        } 
        else if(prettyPrintTimeString.EndsWith(SINGLE_DAY)) {
            return new TimeSpan(1, 0, 0, 0);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_DAY)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_DAY, "").Trim(), out int days)) {
                return new TimeSpan(days, 0, 0, 0);
            } else {
                return new TimeSpan(1, 0, 0, 0);
            }
        } 
        else if(prettyPrintTimeString.EndsWith(SINGLE_MONTH)) {
            return new TimeSpan(30, 0, 0, 0);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_MONTH)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_MONTH, "").Trim(), out int months)) {
                return new TimeSpan(months * 30, 0, 0, 0);
            } else {
                return new TimeSpan(30, 0, 0, 0);
            }
        } 
        else if(prettyPrintTimeString.EndsWith(SINGLE_YEAR)) {
            return new TimeSpan(365, 0, 0, 0);
        } 
        else if(prettyPrintTimeString.EndsWith(PLURAL_YEAR)) {
            if(int.TryParse(prettyPrintTimeString.Replace(PLURAL_YEAR, "").Trim(), out int years)) {
                return new TimeSpan(years * 365, 0, 0, 0);
            } else {
                return new TimeSpan(365, 0, 0, 0);
            }
        }

        return TimeSpan.Zero;
    }

    public static string ReturnWords(string text, int startWordIdx, int endWordIdx) {
        int wordCount = 0, index = 0;
        int startIdx = -1;
        int endIdx = -1;
        int wordLength;

        // skip whitespace until first word
        while(index < text.Length && char.IsWhiteSpace(text[index])) {
            index++;
        }

        while(index < text.Length) {
            wordLength = 0;

            // check if current char is part of a word
            while(index < text.Length && !char.IsWhiteSpace(text[index])) {
                index++;
                wordLength++;
            }

            wordCount++;

            if(wordCount - 1 == startWordIdx) {
                startIdx = index - wordLength;
            }

            if(wordCount == endWordIdx) {
                endIdx = index - 1;
                break;
            }

            if(endIdx == -1) {
                // skip whitespace until next word
                while(index < text.Length && char.IsWhiteSpace(text[index])) {
                    index++;
                }
            }
        }

        // If the total length of the file is less than the number of words requested, return the whole file
        if(endIdx == -1 && wordCount < endWordIdx) {
            endIdx = text.Length;
        }

        if(startIdx == -1 || endIdx == -1) {
            return null;
        } else {
            return text.Substring(startIdx, endIdx - startIdx);
        }
    }

    public static int GetWordCount(string text) {
        if(string.IsNullOrEmpty(text)) {
            return 0;
        }
        int wordCount = 0, index = 0;

        // skip whitespace until first word
        while(index < text.Length && char.IsWhiteSpace(text[index]))
            index++;

        while(index < text.Length) {
            // check if current char is part of a word
            while(index < text.Length && !char.IsWhiteSpace(text[index]))
                index++;

            wordCount++;

            // skip whitespace until next word
            while(index < text.Length && char.IsWhiteSpace(text[index]))
                index++;
        }

        return wordCount;
    }

    public static int GetTokenCount(string message, string model = "gpt-4") {
        TikToken tikToken = TikToken.EncodingForModel(model);
        return tikToken.Encode(message).Count;
    }

    // https://stackoverflow.com/questions/6944056/compare-string-similarity
    public static int ComputeLevenshteinDistance(string s, string t) {
        if(string.IsNullOrEmpty(s)) {
            if(string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if(string.IsNullOrEmpty(t)) {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for(int i = 0; i <= n; d[i, 0] = i++)
            ;
        for(int j = 1; j <= m; d[0, j] = j++)
            ;

        for(int i = 1; i <= n; i++) {
            for(int j = 1; j <= m; j++) {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }


    public static bool GetDateTimeFromString(string dateTimeSource, out DateTime result) {
        // find the index of the first and last numbers in the filename
        int firstNumberIndex = -1;
        int lastNumberIndex = -1;
        for(int y = 0; y < dateTimeSource.Length; y++) {
            if(firstNumberIndex == -1 && char.IsNumber(dateTimeSource[y])) {
                firstNumberIndex = y;
                break;
            }
        }
        for(int y = dateTimeSource.Length - 1; y >= 0; y--) {
            if(lastNumberIndex == -1 && char.IsNumber(dateTimeSource[y])) {
                lastNumberIndex = y + 1;
                break;
            }
        }

        if(firstNumberIndex == -1 || lastNumberIndex == -1) {
            result = default;
            Debug.LogWarning($"Unable to parse DateTime from string [{dateTimeSource}]. First idx [{firstNumberIndex}]. Second Idx [{lastNumberIndex}]");
            return false;
        }

        // make a substring of the numbers
        string numberSubstring = dateTimeSource.Substring(firstNumberIndex, lastNumberIndex - firstNumberIndex);

        // Try to parse the numbers as a timestamp
        numberSubstring = numberSubstring.Replace('_', ' ');
        
        List<string> elements = new List<string>(numberSubstring.Split(' '));
        if(elements.Count != 2) {
            // Somehow some other data got mixed in. Lovely. Let's figure out which bits are the ones we want

            // First pass, remove any elements that are not the expected length. Date should have 10 chars and time should have 8
            elements.RemoveAll(e => e.Length != 10 && e.Length != 8);

            if(elements.Count != 2) {
                // Second pass?
            } else {
                numberSubstring = elements[0] + " " + elements[1];
            }
        }

        // Replace the last two dashes with colons
        numberSubstring = numberSubstring.Substring(0, numberSubstring.Length - 3) + ":" + numberSubstring.Substring(numberSubstring.Length - 2);
        numberSubstring = numberSubstring.Substring(0, numberSubstring.Length - 6) + ":" + numberSubstring.Substring(numberSubstring.Length - 5);

        return DateTime.TryParse(numberSubstring, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out result);
    }

    public static string ConvertOpenAIChatMessageToJson(Message message) {
        return $"{{\"role\":\"{message.Role}\",\"content\":\"{(message.Content is string str ? str : "<Not string>").Replace("\n", "\\n")}\"}}";
    }

    public static string ConvertOpenAIChatMessagesListToJson(List<Message> messages) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for(int x = 0; x < messages.Count; x++) {
            sb.Append(ConvertOpenAIChatMessageToJson(messages[x]));
            if(x < messages.Count - 1) {
                sb.Append(",");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    private static string[] units = { "zero", "one", "two", "three",
        "four", "five", "six", "seven", "eight", "nine", "ten", "eleven",
        "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen" };

    private static string[] tens = { "", "", "twenty", "thirty", "forty",
        "fifty", "sixty", "seventy", "eighty", "ninety" };

    private const string MINUS_PREFIX = "minus ";

    private const string HUNDRED_WORD = "hundred";
    private const string THOUSAND_WORD = "thousand";
    private const string MILLION_WORD = "million";
    private const string BILLION_WORD = "billion";

    private const int TEN = 10;
    private const int HUNDRED = 100;
    private const int THOUSAND = 1000;
    private const int MILLION = 1000000;
    private const int BILLION = 1000000000;

    /// <summary>
    /// Returns the lowercase english string for the number provided. Supports the full range of int values. 
    /// </summary>
    public static string GetWordFromNumber(int i) {
        StringBuilder sb = new StringBuilder();
        GetWordFromNumber(i, sb);
        return sb.ToString();
    }

    private static void GetWordFromNumber(int i, StringBuilder sb) {
        string prefix = string.Empty;
        if(i < 0) {
            prefix = MINUS_PREFIX;
            i *= -1;
        }

        // i is between 1 and 19
        if(i < units.Length) {
            sb.Append(prefix);
            sb.Append(units[i]);
            return;
        }
        // i is between 20 and 99
        if(i < HUNDRED) {
            sb.Append(prefix);
            sb.Append(tens[i / TEN]);
            if(i % TEN > 0) {
                sb.Append(" ");
                GetWordFromNumber(i % TEN, sb);
            }
            return;
        }
        // i is between 100 and 999
        if(i < THOUSAND) {
            sb.Append(prefix);
            sb.Append(units[i / HUNDRED]);
            sb.Append(" ");
            sb.Append(HUNDRED_WORD);
            if(i % HUNDRED > 0) {
                sb.Append(" and ");
                GetWordFromNumber(i % HUNDRED, sb);
            }
            return;
        }
        // i is between 1000 and 999999
        if(i < MILLION) {
            sb.Append(prefix);
            sb.Append(GetWordFromNumber(i / THOUSAND));
            sb.Append(" ");
            sb.Append(THOUSAND_WORD);
            if(i % THOUSAND > 0) {
                sb.Append(" ");
                GetWordFromNumber(i % THOUSAND, sb);
            }
            return;
        }
        // i is between 1 million and 999999999
        if(i < BILLION) {
            sb.Append(prefix);
            GetWordFromNumber(i / MILLION, sb);
            sb.Append(" ");
            sb.Append(MILLION_WORD);
            if(i % MILLION > 0) {
                sb.Append(" ");
                GetWordFromNumber(i % MILLION, sb);
            }
            return;
        }
        // i is between 1 billion and int.MaxValue
        sb.Append(prefix);
        GetWordFromNumber(i / BILLION, sb);
        sb.Append(" ");
        sb.Append(BILLION_WORD);
        if(i % BILLION > 0) {
            sb.Append(" ");
            GetWordFromNumber(i % BILLION, sb);
        }
        return;
    }
}
