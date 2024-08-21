public enum EOperationType {

    /// <summary>
    /// Forces the operation to be completed as a single Text Completion 
    /// without the possibility of being expanded into a sub-project.
    /// </summary>
    /* An example of a text completion operation:
    <Operation id="UnderstandTask" type="TextCompletion" desc="Analyze the task and extract key objectives.">
        <Input id="taskDescription"/>
        <Output id="taskObjectives" path="analysis/objectives.txt"/>
    </Operation> 
    */
    TextCompletion,

    /// <summary>
    /// Forces the operation to be completed as a new sub-project without 
    /// the possibility of being completed in a single Text Completion.
    /// </summary>
    /* An example of a project completion operation:
    <Operation  id="DraftNovel" type="ProjectCompletion" desc="Draft a novel based on the provided task objectives.">
        <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
        <Input id="taskObjectives"/>
        <Output id="chapter_{{ChapterIterator}}" path="draft/chapter_{{ChapterIterator}}.txt"/>
    </Operation> 
    */
    ProjectCompletion,

    /// <summary>
    /// Specifies that the desired Project Completion is already defined 
    /// and can be found at the path provided.
    /// </summary>
    /* A pre-exisitng TC operation:
    <Operation id="ReviseChapter" type="PreExisting" path="TCXMLs/CreativeWriting/revise_chapter.tcxml">
        <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
        <Input id="chapter_{{ChapterIterator}}"/>
        <Input id="chapter_{{ChapterIterator}}_review"/>
        <Output id="chapter_{{ChapterIterator}}" path="revision/chapter_{{ChapterIterator}}.txt"/>
    </Operation> 
    */
    PreExisting,

    /// <summary>
    /// Uses an LLM AI to dynamically determine if it would be 
    /// better to use a TextCompletion or a ProjectCompletion, 
    /// depending on the provided context.
    /// </summary>
    /* An example of a dynamic operation:
    <Operation id="ReviewChapter" type="DynamicCompletion" desc="Review chapter drafts.">
        <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
        <Input id="draft_chapter_{{ChapterIterator}}"/>
        <Output id="chapter_{{ChapterIterator}}_review" path="review/chapter_{{ChapterIterator}}_review.txt"/>
    </Operation> 
    */
    DynamicCompletion,

    /// <summary>
    /// Used to define a choice that will use an LLM AI to decide 
    /// between two or more options. The result will determine what 
    /// operation to move onto next (rather than progress sequentially).
    /// </summary>
    /* An example of a logic gate operation:
    <Operation id="DecideNextStep" type="LogicGate" desc="Decide if the novel is good enough to fulfil the task description.">
        <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
        <Input id="taskDescription">
        <Input id="chapter_{{ChapterIterator}}_review"/>
        <Options>
            <Option goto="ReviseChapter" condition="novel requires revisions"/>
            <Option goto="[NEXT]"/>
        </Options>
    </Operation>
    */
    LogicGate,

    /// <summary>
    /// Indicates that this operation should perform a pre-defined 
    /// function that does not require the involvement of an LLM AI.
    /// </summary>
    /* A machine function operation:
    <Operation id="CombineDocument" type="ExecuteFunction" func="combine_document">
        <Iterator id="ChapterIterator" from="1" to="{{ChapterCount}}"/>
		<Input id="revised_chapter_{{ChapterIterator}}"/>
		<Output id="final_novel" path="final_novel.txt"/>
    </Operation> 
    */
    ExecuteFunction
}