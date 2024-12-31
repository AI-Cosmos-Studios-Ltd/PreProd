using System.Text;
using UnityEngine;

public class CognitiveEngineManager : MonoSingleton<CognitiveEngineManager>, IDebugInfo {

    protected override void OnInstanceInitialised() {
        RunTest();
    }

    protected override void OnInstanceDestroyed() {
        
    }


    private void RunTest() {
        // we have an example TTML here: Assets\Data\TTML\TTML_Example1_v10.xml
        // We want to load it into a TTMLData object and then run the operations

        // Load the TTML file
        TTMLData ttmlData = TTMLData.Load("Assets/Data/TTML/TTML_Example1_v10.xml");   
        
        StringBuilder stringBuilder = new StringBuilder();
        ttmlData.GetDebugInfo(stringBuilder);
        Debug.Log(stringBuilder.ToString());

        CognitiveEngineModule testModule = new CognitiveEngineModule(ttmlData);
        testModule.SetWorkingDirectory("Assets/Data/TTML/");
        if(testModule.Validate()) {
            Debug.Log("Validation successful");
        } else {
            Debug.LogError("Validation failed");
        }
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        
    }
}
