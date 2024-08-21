using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITCDataConverter<T> {
    public bool ImportFromString(T dataToImport, out TaskCompletionData tcData);
    public bool ExportToString(TaskCompletionData data, out T exportedData);
}
