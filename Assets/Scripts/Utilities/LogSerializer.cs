#if(!UNITY_WEBPLAYER || UNITY_EDITOR)
#define PERSISTANT_DATA_IO_ENABLED
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if PERSISTANT_DATA_IO_ENABLED
using System.IO;
using System.Text;
using UnityEditor;
#endif

public class LogSerializer : MonoSingleton<LogSerializer> {

	private static readonly List<LogType> LOG_TYPES_TO_SAVE = new List<LogType> { 
        LogType.Error, 
        LogType.Assert, 
        LogType.Exception, 
        LogType.Warning, 
    };

	private static readonly bool DO_ADD_TIMESTAMP = true;

#pragma warning disable 0649
    [SerializeField]
    private Text logDisplayText;
#pragma warning restore 0649

#if PERSISTANT_DATA_IO_ENABLED
    private static string LOG_NAME = "GameLog.txt";
    private static string LOG_PREVIOUS_NAME = "GameLog-prev.txt";
    private static string FullLogFilePath { get { return Path.Combine(Application.persistentDataPath, LOG_NAME); } }
    private static string FullPreviousLogFilePath { get { return Path.Combine(Application.persistentDataPath, LOG_PREVIOUS_NAME); } }

    private const string NEW_LINE = "\r\n";
    private const string PAGE_BREAK = "----------------------------------------";
    private const int NUM_STACK_LINES = 2;
    private byte[] newLineBytes;
    private byte[] pageBreakBytes;
    private FileStream logFileStream;
#endif

    private StringBuilder sb;
    private bool doSuspendLogTracking = false;

    protected override void OnInstanceInitialised() {
#if PERSISTANT_DATA_IO_ENABLED
        // Shift any old logs to the previous log file.
        if(File.Exists(FullLogFilePath)) {
            if(File.Exists(FullPreviousLogFilePath)) {
                File.Delete(FullPreviousLogFilePath);
            }
            try {
                File.Move(FullLogFilePath, FullPreviousLogFilePath);
            } catch(Exception e) {
                Debug.LogWarning("Unable to rename log file [" + e.Message + "]");
                File.Delete(FullLogFilePath);
            }
        }

        Debug.Log(FullLogFilePath);

        Application.logMessageReceived += OnLog;

        newLineBytes = new UTF8Encoding(true).GetBytes(NEW_LINE);
        pageBreakBytes = new UTF8Encoding(true).GetBytes(PAGE_BREAK);

        try {
            logFileStream = new FileStream(FullLogFilePath, FileMode.Create);
        } catch(Exception e) {
            Debug.LogWarning("Unable to write log to file [" + e.Message + "]");
            return;
        }
        logFileStream.Flush();

        if(logDisplayText != null) {
            sb = new StringBuilder();
        }
#endif
    }

    protected override void OnInstanceDestroyed() {
        FinaliseLog("OnDestroy");
    }

    void OnApplicationQuit() {
        FinaliseLog("OnApplicationQuit");
    }

    void OnApplicationFocus(bool isFocused) {
#if PERSISTANT_DATA_IO_ENABLED
        if(logDisplayText != null && logFileStream == null) {
            return;
        }

        WriteToLogFile(string.Format("Application focus state changed to [{0}]", isFocused ? "Focused" : "Focus Lost"));

        if(!isFocused && logFileStream != null) {
            logFileStream.Flush();
        }
#endif
    }

    void FinaliseLog(string endOfFile) {
#if PERSISTANT_DATA_IO_ENABLED
        if(logFileStream == null) {
            return;
        }

        WriteToLogFile("LogSerializer destroyed [" + endOfFile + "]");

        logFileStream.Flush();
        logFileStream = null;
#endif
    }

    public void SerializeLogImmediate(string message, LogType type = LogType.Log, bool includeStacktrace = true, bool includeInEditorLog = true) {
        if(includeInEditorLog) {
            SendNonSerializedLog(message, type);
        }

        // Get the stacktrace, but remove the refernce to this function call (as that's not useful information)
        string stacktrace = includeStacktrace ? Environment.StackTrace : null;
        if(stacktrace != null && stacktrace.Length > 1 && stacktrace.IndexOf('\n') != -1) {
            string[] stacktraceLines = stacktrace.Split('\n');
            stacktrace = string.Join("\n", stacktraceLines, 2, Math.Min(stacktraceLines.Length - 2, NUM_STACK_LINES));
        }
        WriteToLogFile(message, stacktrace, type, true);  
    }

    private void SendNonSerializedLog(string message, LogType type = LogType.Log) {
        // We want to make sure this is in the editor log, but we don't want
        // to double-write to file, so we have a suppression flag to prevent this.
        doSuspendLogTracking = true;
        switch(type) {
            case LogType.Log:
                Debug.Log(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Error:
                Debug.LogError(message);
                break;
            case LogType.Exception:
                Debug.LogException(new Exception(message));
                break;
            case LogType.Assert:
                Debug.LogAssertion(message);
                break;
        }
        doSuspendLogTracking = false;
    }

    void OnLog(string message, string stacktrace, LogType type) {
        if(doSuspendLogTracking || !LOG_TYPES_TO_SAVE.Contains(type)) {
            return;
        }

#if PERSISTANT_DATA_IO_ENABLED
        if(stacktrace != null && stacktrace.Length > 0 && stacktrace.IndexOf('\n') != -1) {
            string[] stacktraceLines = stacktrace.Split('\n');
            stacktrace = string.Join("\n", stacktraceLines, 1, Math.Min(stacktraceLines.Length - 1, NUM_STACK_LINES));
            WriteToLogFile(message, stacktrace, type);
        } else {
            WriteToLogFile(message, type: type);
        }

        if(logFileStream != null && type == LogType.Exception) {
            WriteToLogFile("Exception detected! Flushing log file!");
            logFileStream.Flush();
        }
        return;
#endif
    }

    void WriteToLogFile(string newMessage, string stackTrace = null, LogType type = LogType.Log, bool forceFlush = false) {
        if(DO_ADD_TIMESTAMP) {
            newMessage = $"UTC-{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.ff")}\n{newMessage}";
        }

        // TODO: Create a queue system and only serialize to file if the queue is over a threshold, or the serilizaiton is forced.

        if(logDisplayText != null) {
            sb.Append(newMessage);
            sb.Append(NEW_LINE);
            if(type == LogType.Exception && !string.IsNullOrEmpty(stackTrace)) {
                sb.Append(stackTrace);
                sb.Append(NEW_LINE);
            }
            sb.Append(NEW_LINE);
            logDisplayText.text = sb.ToString();
        }

#if PERSISTANT_DATA_IO_ENABLED
        if(logFileStream == null) {
            return;
        }

        byte[] info = new UTF8Encoding(true).GetBytes(newMessage);

        logFileStream.Write(newLineBytes, 0, newLineBytes.Length);
        logFileStream.Write(info, 0, info.Length);
        logFileStream.Write(newLineBytes, 0, newLineBytes.Length);

        if(stackTrace == null || stackTrace.Length == 0) {
            logFileStream.Write(pageBreakBytes, 0, pageBreakBytes.Length);
        } 
        else {
            info = new UTF8Encoding(true).GetBytes(stackTrace);

            logFileStream.Write(info, 0, info.Length);
            logFileStream.Write(newLineBytes, 0, newLineBytes.Length);
            logFileStream.Write(pageBreakBytes, 0, pageBreakBytes.Length);
        }

        if(type == LogType.Exception || forceFlush) {
            FlushLogStream();
        }
#endif
    }

    private void FlushLogStream() {
        if(logFileStream == null || !logFileStream.CanWrite) {
            return;
        }
        logFileStream.Flush();
    }

#if UNITY_EDITOR
    [MenuItem("AI Cosmos/Open Persistant Data Folder")]
    public static void OpenLogOutputFolder() {
        Application.OpenURL(Application.persistentDataPath);
    }

    [MenuItem("AI Cosmos/Open Output Folder")]
    public static void OpenOutputFolder() {
        Application.OpenURL(Application.dataPath + "/../Output/");
    }
#endif

}
