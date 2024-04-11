using UnityEditor;
using UnityEngine;

public static class AICosmosEditorMenu 
{
#if UNITY_EDITOR
    [MenuItem("AI Cosmos/Folders/Output")]
    public static void OpenOutputFolder() {
        Application.OpenURL(Application.dataPath + "/../Output/");
    }

    [MenuItem("AI Cosmos/Folders/Persistant Data")]
    public static void OpenLogOutputFolder() {
        Application.OpenURL(Application.persistentDataPath);
    }
#endif
}
