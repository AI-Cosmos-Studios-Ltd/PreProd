using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static TaskCompletionData;
using static TaskCompletionData.Operation;

public class TCXMLConverter : ITCDataConverter<string> {
    public bool ExportToString(TaskCompletionData data, out string exportedData) {
        if(data == null) {
            exportedData = string.Empty;
            return false;
        }
        
        XElement xmlDoc = new XElement("TCXML");
        xmlDoc.SetAttributeValue("version", data.version);

        // Project element
        XElement projectXML = new XElement("Project");
        projectXML.SetAttributeValue("id", data.project.id);
        projectXML.SetAttributeValue("date", data.project.date);
        projectXML.SetAttributeValue("desc", data.project.desc);
        projectXML.SetAttributeValue("tags", data.project.tags);
        projectXML.SetAttributeValue("author", data.project.author);
        xmlDoc.Add(projectXML);

        // Inputs element
        XElement inputsXML = new XElement("Inputs");
        foreach(TaskCompletionData.Input input in data.inputs) {
            XElement inputXML = new XElement("Input");
            inputXML.SetAttributeValue("id", input.id);
            inputXML.SetAttributeValue("file", input.file);
            inputsXML.Add(inputXML);
        }
        xmlDoc.Add(inputsXML);

        // Vars element
        XElement varsXML = new XElement("Vars");
        foreach(Variable var in data.vars) {
            XElement varXML = new XElement("Var");
            varXML.SetAttributeValue("name", var.name);
            varXML.SetAttributeValue("type", var.type);
            varXML.SetAttributeValue("value", var.value);
            varsXML.Add(varXML);
        }
        xmlDoc.Add(varsXML);

        // Operations element
        XElement operationsXML = new XElement("Operations");
        foreach(Operation operation in data.operations) {
            XElement operationXML = new XElement("Operation");
            operationXML.SetAttributeValue("id", operation.id);
            operationXML.SetAttributeValue("type", operation.type);
            operationXML.SetAttributeValue("desc", operation.desc);

            foreach(Iterator iterator in operation.iterators) {
                XElement iteratorXML = new XElement("Iterator");
                iteratorXML.SetAttributeValue("id", iterator.id);
                iteratorXML.SetAttributeValue("from", iterator.from);
                iteratorXML.SetAttributeValue("to", iterator.to);
                operationXML.Add(iteratorXML);
            }

            foreach(TaskCompletionData.Input input in operation.inputs) {
                XElement inputXML = new XElement("Input");
                inputXML.SetAttributeValue("id", input.id);
                inputXML.SetAttributeValue("file", input.file);
                operationXML.Add(inputXML);
            }

            foreach(Output output in operation.outputs) {
                XElement outputXML = new XElement("Output");
                outputXML.SetAttributeValue("file", output.file);
                operationXML.Add(outputXML);
            }

            operationsXML.Add(operationXML);
        }
        xmlDoc.Add(operationsXML);
        exportedData = xmlDoc.ToString();
        return true;
    }

    public bool ImportFromString(string dataToImport, out TaskCompletionData tcData) {
        tcData = new TaskCompletionData();

        XElement xmlDoc = null;
        try {
            xmlDoc = XElement.Parse(dataToImport);
        } catch(Exception ex) {
            Debug.LogError($"Type: {ex.GetType()}\nMessage: {ex.Message}\nInnerException: {ex.InnerException}\nStackTrace:\n{ex.StackTrace}");
            return false;
        }

        string GetAttributeValue(XElement element, string name) {
            XAttribute attribute = element.Attribute(name);
            if(attribute != null) {
                return attribute.Value;
            }
            return string.Empty;
        }

        // Get the version number
        tcData.version = xmlDoc.Attribute("version").Value;

        // Get the project elements from the doc
        XElement projectXML = xmlDoc.Element("Project");
        tcData.project = new Project();
        if(projectXML != null) {
            tcData.project.id = GetAttributeValue(projectXML, "id");
            tcData.project.date = GetAttributeValue(projectXML, "date");
            tcData.project.desc = GetAttributeValue(projectXML, "desc");
            tcData.project.tags = GetAttributeValue(projectXML, "tags");
            tcData.project.author = GetAttributeValue(projectXML, "author");
        }
        
        // Get the input values from the doc
        tcData.inputs = new List<TaskCompletionData.Input>();
        foreach(XElement inputXML in xmlDoc.Elements("Input")) {
            TaskCompletionData.Input input = new TaskCompletionData.Input();
            input.id = GetAttributeValue(inputXML, "id");
            input.file = GetAttributeValue(inputXML, "file");
            tcData.inputs.Add(input);
        }

        // Get the variable values from the doc
        tcData.vars = new List<TaskCompletionData.Variable>();
        foreach(XElement varXML in xmlDoc.Elements("Var")) {
            TaskCompletionData.Variable var = new TaskCompletionData.Variable();
            var.name = GetAttributeValue(varXML, "name");
            var.type = GetAttributeValue(varXML, "type");
            var.value = GetAttributeValue(varXML, "value");
            tcData.vars.Add(var);
        }

        // Get the operation values from the doc
        tcData.operations = new List<TaskCompletionData.Operation>();
        foreach(XElement operationXML in xmlDoc.Elements("Operation")) {
            TaskCompletionData.Operation operation = new TaskCompletionData.Operation();
            operation.id = GetAttributeValue(operationXML, "id");
            operation.type = GetOperationTypeFromString(GetAttributeValue(operationXML, "type"));
            operation.desc = GetAttributeValue(operationXML, "desc");

            operation.iterators = new List<TaskCompletionData.Iterator>();
            foreach(XElement iteratorXML in operationXML.Elements("Iterator")) {
                TaskCompletionData.Iterator iterator = new TaskCompletionData.Iterator();
                iterator.id = GetAttributeValue(iteratorXML, "id");
                iterator.from = GetAttributeValue(iteratorXML, "from");
                iterator.to = GetAttributeValue(iteratorXML, "to");
                operation.iterators.Add(iterator);
            }

            operation.inputs = new List<TaskCompletionData.Input>();
            foreach(XElement inputXML in operationXML.Elements("Input")) {
                TaskCompletionData.Input input = new TaskCompletionData.Input();
                input.id = GetAttributeValue(inputXML, "id");
                input.file = GetAttributeValue(inputXML, "file");
                operation.inputs.Add(input);
            }

            operation.outputs = new List<TaskCompletionData.Output>();
            foreach(XElement outputXML in operationXML.Elements("Output")) {
                TaskCompletionData.Output output = new TaskCompletionData.Output();
                output.file = GetAttributeValue(outputXML, "file");
                operation.outputs.Add(output);
            }

            tcData.operations.Add(operation);
        }

        // Get the output values from the doc
        tcData.outputs = new List<TaskCompletionData.Output>();
        foreach(XElement outputXML in xmlDoc.Element("Outputs").Elements("Output")) {
            TaskCompletionData.Output output = new TaskCompletionData.Output();
            output.file = GetAttributeValue(outputXML, "file");
            tcData.outputs.Add(output);
        }

        return true;
    }

    private EOperationType GetOperationTypeFromString(string typeString) {
        switch(typeString.ToLower()){
            case "task":
            case "taskcompletion":
                return EOperationType.TextCompletion;

            case "execute":
            case "function":
            case "executefunction":
                return EOperationType.ExecuteFunction;

            case "preexisting":
            case "preexistingcompletion":
                return EOperationType.PreExisting;

            default:
                return EOperationType.DynamicCompletion;
        }
    }


    /*
     * Example TCXML file:

    <TCXML version="0.10.0">
        <!-- Project Definition -->
        <Project id="FictionProject" tags="Novel" author="gpt-4-1106-preview" date="2023-04-01" desc="Develop a novel with a compelling narrative arc."/>

        <!-- Inputs -->
        <Inputs>
          <Input id="story_requirements" file="story_requirements.txt"/>
          <!-- Additional inputs as necessary -->
        </Inputs>

        <!-- Project Variables -->
        <Vars>
          <Var name="CharacterCount" type="integer" value="4"/>
          <Var name="SettingType" type="string" value="Mystery"/>
          <Var name="ChapterCount" type="integer" value="12"/>
          <!-- Additional variables as necessary -->
        </Vars>

        <!-- Operations -->
        <Operations>
          <!-- Operation for concept development -->
          <Operation id="ConceptDevelopment" type="TaskCompletion" desc="Develop concepts for characters and setting based on provided requirements.">
            <Iterator id="CharacterIterator" from="1" to="{{CharacterCount}}"/>
            <Input id="story_requirements"/>
            <Output id="character_{{CharacterIterator}}_concept" file="character_{{CharacterIterator}}_concept.xml"/>
            <Output id="setting_concept" file="concept_setting.xml"/>
          </Operation>

          <!-- Operation for drafting chapters -->
          <Operation id="DraftChapter" type="TaskCompletion" desc="Generate draft for the chapter based on developed concepts.">
            <Iterator id="CharacterIterator" from="1" to="{{CharacterCount}}"/>
            <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
            <Input id="story_requirements"/>
            <Input id="character_{{CharacterIterator}}_concept"/>
            <Input id="setting_concept"/>
            <Output id="draft_chapter_{{ChapterIterator}}" file="draft_chapter_{{ChapterIterator}}.xml"/>
          </Operation>

          <!-- Operation for revising chapters -->
          <Operation id="ReviseChapter" type="TaskCompletion" desc="Revise and polish chapter drafts.">
            <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
            <Input id="draft_chapter_{{ChapterIterator}}"/>
            <Output file="revised_chapter_{{ChapterIterator}}.xml"/>
          </Operation>

          <!-- Function Invocation for final compilation -->
          <Operation id="CompileNovel" type="Function" desc="Compile all revised chapters into a single novel document.">
            <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
            <Input id="revised_chapter_{{ChapterIterator}}"/>
            <Output file="final_novel.xml"/>
          </Operation>

          <!-- Additional operations as necessary -->
        </Operations>

        <!-- Final Output Definition -->
        <Outputs>
          <Output file="final_novel.xml"/>
        </Outputs>
    </TCXML>
    */
}
