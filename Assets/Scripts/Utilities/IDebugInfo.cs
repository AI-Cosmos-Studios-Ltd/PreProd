using System.Text;

public interface IDebugInfo {
    public void GetDebugInfo(StringBuilder sb, string prefix = "");
}
