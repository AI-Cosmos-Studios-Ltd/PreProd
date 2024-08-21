using System.Collections.Generic;

/// <summary>
/// This class holds the information for a task completion operation.
/// It can be converted to a TCXML file and back again.
/// </summary>
public class TaskCompletionData {
    public string version;

    // Project info
    public class Project {
        public string id;
        public string date;
        public string desc;
        public string tags;
        public string author;
    }
	public Project project;

	// Input definitions
	public class Input {
        public string id;
        public string file;
    }
	public List<Input> inputs;

	// Variable definitions
	public class Variable {
        public string name;
        public string type;
        public string value;
    }
	public List<Variable> vars;

    // Operation definitions
    public class Operation {
        public string id;
        public EOperationType type;
        public string desc;
        
        public List<Iterator> iterators;
        public List<Input> inputs;
        public List<Output> outputs;
        }
        public class Iterator {
        public string id;
        public string from;
        public string to;
    }
    public List<Operation> operations;

	// Output definitions
	public class Output {
        public string file;
    }
    public List<Output> outputs;
}
