using System.Text;

public class CognitiveEngineManager : MonoSingleton<CognitiveEngineManager>, IDebugInfo {

    protected override void OnInstanceInitialised() {

    }

    protected override void OnInstanceDestroyed() {
        
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        
    }
}
