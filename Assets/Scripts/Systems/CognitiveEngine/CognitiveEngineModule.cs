using System.IO;
using UnityEngine;

public class CognitiveEngineModule 
{
    public readonly TTMLData TTMLData;

    private string workingDirectory;

    public CognitiveEngineModule(TTMLData ttmlData) {
        TTMLData = ttmlData;
    }

    public void SetWorkingDirectory(string newDirectory) {
        workingDirectory = newDirectory;
    }

    public bool Validate() {
        bool isValid = true;

        // Confirm we have a valid TTMLData object
        if(TTMLData == null) {
            Debug.LogError("Validation:\nNo TTMLData object has been provided");
            isValid = false;
        }

        // Confirm the presence of a working directory
        if(string.IsNullOrEmpty(workingDirectory)) {
            Debug.LogError("Validation:\nNo working directory has been provided");
            isValid = false;
        }

        // Confirm the presence of input files listed in the TTML file, using
        // the working directory as a base if a relative path has been provided
        foreach(TTMLFile inputFile in TTMLData.InputFiles) {
            string fullPath = Path.Combine(inputFile.Folder,inputFile.Id) + inputFile.Extension;
            if(!Path.IsPathRooted(fullPath)) {
                fullPath = Path.Combine(workingDirectory, fullPath);
            }
            if(!File.Exists(fullPath)) {
                Debug.LogError("Validation:\nInput file not found: " + fullPath);
                isValid = false;
            }
        }

        return isValid;
    }
}
