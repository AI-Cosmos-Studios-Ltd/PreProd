using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

[XmlRoot("TTML")]
public class TTMLData : IDebugInfo {
    [XmlAttribute("version")]
    public string Version { get; set; }

    [XmlElement("Task")]
    public TTMLTask Task { get; set; }

    [XmlArray("Vars")]
    [XmlArrayItem("Var")]
    public List<TTMLVar> Vars { get; set; }

    [XmlArray("Iterators")]
    [XmlArrayItem("Iterator")]
    public List<TTMLIterator> Iterators { get; set; }

    [XmlArray("Input")]
    [XmlArrayItem("File")]
    public List<TTMLFile> InputFiles { get; set; }

    [XmlArray("Operations")]
    [XmlArrayItem("Operation")]
    public List<TTMLOperation> Operations { get; set; }

    [XmlElement("Output")]
    public TTMLOutput Output { get; set; }

    public static TTMLData Load(string path) {
        TTMLData tTMLData = null;

        try {
            XmlSerializer serializer = new XmlSerializer(typeof(TTMLData));
            System.IO.StreamReader reader = new System.IO.StreamReader(path);
            tTMLData = (TTMLData)serializer.Deserialize(reader);
            reader.Close();
        } 
        catch(Exception e) {
            UnityEngine.Debug.LogError($"Failed to load TTML file: {path} - {e.Message}");
        }

        return tTMLData;
    }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLData");
        sb.AppendLine($"{prefix}\tVersion [{Version}]");
        Task.GetDebugInfo(sb, prefix + "\t");
        foreach(var var in Vars) {
            var.GetDebugInfo(sb, prefix + "\t");
        }
        foreach(var iterator in Iterators) {
            iterator.GetDebugInfo(sb, prefix + "\t");
        }
        foreach(var file in InputFiles) {
            file.GetDebugInfo(sb, prefix + "\t");
        }
        foreach(var operation in Operations) {
            operation.GetDebugInfo(sb, prefix + "\t");
        }
        Output.GetDebugInfo(sb, prefix + "\t");
    }
}

public class TTMLTask : IDebugInfo {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("desc")]
    public string Description { get; set; }

    [XmlAttribute("genre")]
    public string Genre { get; set; }

    [XmlAttribute("author")]
    public string Author { get; set; }

    [XmlAttribute("date")]
    public DateTime Date { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLTask");
        sb.AppendLine($"{prefix}\tId [{Id}]");
        sb.AppendLine($"{prefix}\tDescription [{Description}]");
        sb.AppendLine($"{prefix}\tGenre [{Genre}]");
        sb.AppendLine($"{prefix}\tAuthor [{Author}]");
        sb.AppendLine($"{prefix}\tDate [{Date}]");
    }
}

public class TTMLVar : IDebugInfo {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLVar");
        sb.AppendLine($"{prefix}\tName [{Name}]");
        sb.AppendLine($"{prefix}\tType [{Type}]");
        sb.AppendLine($"{prefix}\tValue [{Value}]");
    }
}

public class TTMLIterator : IDebugInfo {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("from")]
    public string From { get; set; }

    [XmlAttribute("to")]
    public string To { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLIterator");
        sb.AppendLine($"{prefix}\tId [{Id}]");
        sb.AppendLine($"{prefix}\tFrom [{From}]");
        sb.AppendLine($"{prefix}\tTo [{To}]");
    }
}

public class TTMLFile : IDebugInfo {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("folder")]
    public string Folder { get; set; }

    [XmlAttribute("extension")]
    public string Extension { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLFile");
        sb.AppendLine($"{prefix}\tId [{Id}]");
        sb.AppendLine($"{prefix}\tFolder [{Folder}]");
        sb.AppendLine($"{prefix}\tExtension [{Extension}]");
    }
}

public class TTMLFileRef : IDebugInfo {
    [XmlAttribute("id")]
    public string Id { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLFileRef");
        sb.AppendLine($"{prefix}\tId [{Id}]");
    }
}

public class TTMLOperation : IDebugInfo {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("desc")]
    public string Description { get; set; }

    [XmlElement("Input")]
    public TTMLOperationInput Input { get; set; }

    [XmlElement("Output")]
    public TTMLOperationOutput Output { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLOperation");
        sb.AppendLine($"{prefix}\tId [{Id}]");
        sb.AppendLine($"{prefix}\tType [{Type}]");
        sb.AppendLine($"{prefix}\tDescription [{Description}]");
        Input.GetDebugInfo(sb, prefix + "\t");
        Output.GetDebugInfo(sb, prefix + "\t");
    }
}

public class TTMLOperationInput : IDebugInfo {
    [XmlElement("FileRef")]
    public List<TTMLFileRef> FileRefs { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLOperationInput");
        foreach(var fileRef in FileRefs) {
            fileRef.GetDebugInfo(sb, prefix + "\t");
        }
    }
}

public class TTMLOperationOutput : IDebugInfo {
    [XmlElement("File")]
    public List<TTMLFile> Files { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLOperationOutput");
        foreach(var file in Files) {
            file.GetDebugInfo(sb, prefix + "\t");
        }
    }
}

public class TTMLOutput : IDebugInfo {
    [XmlElement("FileRef")]
    public List<TTMLFileRef> FileRefs { get; set; }

    public void GetDebugInfo(StringBuilder sb, string prefix = "") {
        sb.AppendLine($"{prefix}TTMLOutput");
        foreach(var fileRef in FileRefs) {
            fileRef.GetDebugInfo(sb, prefix + "\t");
        }
    }
}