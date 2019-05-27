using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class The Defines State Machine Scriptable object
/// </summary>
[CreateAssetMenu(fileName = "StateMachine", menuName = "ScriptableObjects/StateMachine", order = 1)]
public class StateMachine : ScriptableObject {

    Dictionary<string, State> state_dictionary;
    Dictionary<State, List<Transition>> transition_dictionary;

    [SerializeField] List<State> states;
    [SerializeField] List<Transition> transitions;
    [SerializeField] List<Parameter> parameters;

    List<string> parameter_names;

    [SerializeField] State entry_state;

    public StateMachine() {
        states = new List<State>();
        transitions = new List<Transition>();
        parameters = new List<Parameter>();

        Init();
    }

    /// <summary>
    /// Assigns new ids to states
    /// Removes invalid transitions
    /// Loads Dicitionaries
    /// Assigns Entry State
    /// </summary>
    public void Init() {
        for (int i = 0; i < states.Count; i++) {
            states[i].id = i;
        }

        for (int i = 0; i < transitions.Count; i++) {
            for (int j = transitions[i].conditions.Count - 1; j >= 0; j--) {
                Condition c = transitions[i].conditions[j];
                int index = parameters.IndexOf(c.parameter);
                if (index != -1) {
                    c.parameter = parameters[index];
                } else {
                    transitions[i].RemoveCondition(c);
                }
            }
        }

        LoadDictionaries();

        if (states.Count > 0) {
            if (state_dictionary.ContainsKey(entry_state.name)) {
                entry_state = state_dictionary[entry_state.name];
            } else {
                entry_state = states[0];
            }
        } else {
            entry_state = null;
        }
    }

    /// <summary>
    /// Creates an Object to track position in this state machine
    /// </summary>
    /// <returns>Instance of StateMachine tracking object</returns>
    public Instance GetStateMachineInstance() {
        return new Instance(this);
    }

    public State GetEntryState() {
        return entry_state;
    }

    public void SetEntryState(State entry) {
        entry_state = entry;
    }

    public List<State> GetStateList() {
        return new List<State>(states);
    }

    /// <summary>
    /// Creates a new state, if the name already exists appends numbers on the end
    /// until the new name doesn not exist
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Newly created state</returns>
    public State CreateState(string name) {
        State new_state = new State(0, new Rect(), name);

        int count = 0;
        while (state_dictionary.ContainsKey(new_state.name)) {
            new_state = new State(0, new Rect(), name + " " + count);
            count++;
        }

        states.Add(new_state);
        state_dictionary.Add(new_state.name, new_state);
        return new_state;
    }

    public void DeleteState(State s) {
        if (state_dictionary.ContainsKey(s.name)) {
            RemoveAllTransitionsContainingState(s);
            states.Remove(s);
            state_dictionary.Remove(s.name);
        }
    }

    public void RenameState(State state, string name) {
        state.name = name;
        LoadDictionaries();
    }

    /// <summary>
    /// Creates a one way transition between to States
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void CreateTransition(State from, State to) {
        if (!state_dictionary.ContainsKey(from.name) || !state_dictionary.ContainsKey(to.name)) {
            return;
        }
        if (transition_dictionary.ContainsKey(from)) {
            foreach (Transition t in transition_dictionary[from]) {
                if (t.to == to) {
                    return;
                }
            }
        }

        Transition new_transition = new Transition(from, to);
        transitions.Add(new_transition);
        InsertTransitionIntoDictionary(new_transition);
    }

    /// <summary>
    /// Deletes a transition between two states if it exists
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void DeleteTransition(State from, State to) {
        if (!state_dictionary.ContainsKey(from.name) || !state_dictionary.ContainsKey(to.name)) {
            return;
        }

        Transition to_remove = null;
        foreach (Transition t in transition_dictionary[from]) {
            if (t.to == to) {
                to_remove = t;
            }
        }

        if (to_remove != null) {
            transition_dictionary[from].Remove(to_remove);
            transitions.Remove(to_remove);
        }
    }

    public void DeleteTransition(Transition transition) {
        if (!state_dictionary.ContainsKey(transition.from.name) || !state_dictionary.ContainsKey(transition.to.name)) {
            return;
        }

        transition_dictionary[transition.from].Remove(transition);
        transitions.Remove(transition);
    }

    /// <summary>
    /// Gets all transitions leaving State
    /// </summary>
    /// <param name="s">State</param>
    /// <returns>List of transitions leaving state</returns>
    public List<Transition> GetTransitions(State s) {
        List<Transition> to_return = new List<Transition>();
        if (transition_dictionary.ContainsKey(s)) {
            to_return.AddRange(transition_dictionary[s]);
        }
        return to_return;
    }

    public Transition GetTransition(State from, State to) {
        if (transition_dictionary.ContainsKey(from)) {
            foreach (Transition t in transition_dictionary[from]) {
                if (t.to == to) {
                    return t;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Removes All transitions to or from state
    /// </summary>
    /// <param name="state"></param>
    public void RemoveAllTransitionsContainingState(State state) {
        if (transition_dictionary.ContainsKey(state)) {
            foreach (Transition t in transition_dictionary[state]) {
                transitions.Remove(t);
            }
            transition_dictionary.Remove(state);
        }

        foreach (State s in states) {
            if (transition_dictionary.ContainsKey(s)) {
                for (int i = transition_dictionary[s].Count - 1; i >= 0; i--) {
                    if (transition_dictionary[s][i].to == state) {
                        transition_dictionary[s].RemoveAt(i);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Adds new paramater if name is taken appends numbers onto end
    /// until name is valid
    /// </summary>
    /// <param name="name"></param>
    public void AddParameter(string name) {
        Parameter new_parameter = new Parameter(ValidatedParameterName(name));
        parameters.Add(new_parameter);
    }

    /// <summary>
    /// Renames paramater if new name is taken appends numbers onto end
    /// until name is valid
    /// </summary>
    /// <param name="name"></param>
    public void RenameParameter(Parameter param, string new_name) {
        if (param.name == new_name) {
            return;
        }
        param.name = ValidatedParameterName(new_name);
    }

    public void MoveParameter(int index_from, int index_to) {
        parameters.Move(index_from, index_to);
    }

    public void DeleteParameter(string name) {
        Parameter to_remove = new Parameter(name);

        foreach (Transition t in transitions) {
            t.RemoveCondition(to_remove);
        }

        parameters.Remove(to_remove);
    }

    public List<Parameter> GetParameters() {
        return new List<Parameter>(parameters);
    }
    /// <summary>
    /// Gets list of all parameter names is State Machine
    /// </summary>
    /// <returns>List parameter names as strings</returns>
    public List<string> GetParameterNames() {
        List<string> names = new List<string>();
        foreach (Parameter p in parameters) {
            names.Add(p);
        }
        return names;
    }

    public void ReorderTransition(State state, int old_index, int new_index) {
        transition_dictionary[state].Move(old_index, new_index);
        ReorderTransitions();
    }

    void OnEnable() {
        Init();
    }

    void ReorderTransitions() {
        transitions.Clear();

        foreach (KeyValuePair<State, List<Transition>> kvp in transition_dictionary) {
            foreach (Transition t in kvp.Value) {
                transitions.Add(t);
            }
        }
    }

    void InsertTransitionIntoDictionary(Transition t) {
        if (transition_dictionary.ContainsKey(t.from)) {
            transition_dictionary[t.from].Add(t);
        } else {
            transition_dictionary.Add(t.from, new List<Transition>() { t });
        }
    }

    string ValidatedParameterName(string name) {
        if (IsParameterNameTaken(name)) {
            int count = 0;
            while (IsParameterNameTaken(name + " " + count)) {
                count++;
            }
            return name + " " + count;
        }
        return name;
    }

    bool IsParameterNameTaken(string name) {
        return parameters.Contains(new Parameter(name));
    }

    void LoadDictionaries() {
        state_dictionary = new Dictionary<string, State>();
        foreach (State s in states) {
            state_dictionary.Add(s.name, s);
        }

        transition_dictionary = new Dictionary<State, List<Transition>>();
        List<Transition> to_delete = new List<Transition>();
        foreach (Transition t in transitions) {
            if (!state_dictionary.ContainsKey(t.from.name) || !state_dictionary.ContainsKey(t.to.name)) {
                to_delete.Add(t);
                continue;
            }
            t.from = state_dictionary[t.from.name];
            t.to = state_dictionary[t.to.name];

            InsertTransitionIntoDictionary(t);
        }
        foreach (Transition t in to_delete) {
            transitions.Remove(t);
        }
    }

    /// <summary>
    /// Class that defines an instane of a state machince
    /// Must Set Parameter callbacks before use
    /// </summary>
    public class Instance {
        StateMachine machine;
        public State current_state { get; private set; }

        Dictionary<Parameter, System.Func<bool>> parameter_callbacks;

        public Instance(StateMachine machine) {
            this.machine = machine;
            current_state = machine.entry_state;
            parameter_callbacks = new Dictionary<Parameter, System.Func<bool>>();
        }

        /// <summary>
        /// Sets parameter callbacks so that they can be read to judge transitions
        /// </summary>
        /// <param name="callbacks">Dictionary of Parameter to Bool Callbacks</param>
        public void SetCallbacks(Dictionary<Parameter, System.Func<bool>> callbacks) {
            parameter_callbacks = callbacks;
        }

        /// <summary>
        /// Transition State machine to next state using supplied parameter bool dictionary
        /// </summary>
        /// <param name="parameter_values"></param>
        public void Tansition(Dictionary<Parameter, bool> parameter_values) {
            foreach (Transition t in machine.GetTransitions(current_state)) {
                bool should_transition = true;
                foreach (Condition c in t.conditions) {
                    if (!c.IsConditionMet(parameter_values[c.parameter])) {
                        should_transition = false;
                    }
                }
                if (should_transition) {
                    current_state = t.to;
                    return;
                }
            }
        }

        /// <summary>
        /// Transition State machine to next state using previously set callbacks
        /// See: SetCallbacks
        /// </summary>
        public void TransitionUsingCallbacks() {
            foreach (Transition t in machine.GetTransitions(current_state)) {
                //Debug.Log("Checking transition " + t.from.name + " : " + t.to.name);
                bool should_transition = true;
                foreach (Condition c in t.conditions) {
                    //Debug.Log("Checking if Condition " + c.parameter.name + " is " + c.should_parameter_be_true + " : parameter returned " + parameter_callbacks[c.parameter].Invoke());
                    if (!c.IsConditionMet(parameter_callbacks[c.parameter].Invoke())) {
                        should_transition = false;
                    }
                }
                if (should_transition) {
                    current_state = t.to;
                    return;
                }
            }
        }
    }
}

/// <summary>
/// Class Defining Parameter
/// simply string wrappend in a class and some accessors to allow passing value by reference
/// so parameter name can be altered at all referenced places easily
/// </summary>
[System.Serializable]
public class Parameter {
    [SerializeField] string _name;

    public string name {
        get { return _name; }
        set { _name = value; }
    }

    public Parameter(string name) {
        _name = name;
    }

    public bool GetParameter() {
        return true;
    }

    public override bool Equals(object o) {
        Parameter parameter = o as Parameter;
        if (parameter == null) {
            return false;
        }
        return parameter.name == name;
    }
    public override int GetHashCode() {
        return _name.GetHashCode();
    }

    public static implicit operator string(Parameter p) {
        return p.name;
    }
}

/// <summary>
/// Class Defining State
/// string wrappend in a class and some accessors to allow passing value by reference
/// so parameter name can be altered at all referenced places easily
/// 
/// Contains Rect so that State Machine Editor can save the position of states between uses
/// </summary>
[System.Serializable]
public class State {
    [SerializeField] string _name;

    public string name { get { return _name; } set { _name = value; } }
    public int id { get; set; }
    public Rect rect;

    public State(int id, Rect rect, string name) {
        this.id = id;

        this.rect = rect;
        this.name = name;
    }

    public override bool Equals(object o) {
        State state = o as State;
        if (state == null) {
            return false;
        }
        return state.name == name;
    }
    public override int GetHashCode() {
        return _name.GetHashCode();
    }
}

/// <summary>
/// Class Defining Transition
/// Contains an origin and a destination state and a list of conditions that govern whether the 
/// transition is valid
/// </summary>
[System.Serializable]
public class Transition {
    [SerializeField] State _to, _from;

    [SerializeField] List<Condition> _conditions;

    public State from { get { return _from; } set { _from = value; } }
    public State to { get { return _to; } set { _to = value; } }

    public List<Condition> conditions { get { return new List<Condition>(_conditions); } }

    public Transition(State from, State to) {
        this.from = from;
        this.to = to;
        _conditions = new List<Condition>();
    }

    public void AddCondition(Condition c) {
        _conditions.Add(c);
    }

    public void RemoveCondition(Condition c) {
        _conditions.Remove(c);
    }

    public void ReorderCondition(int old_index, int new_index) {
        _conditions.Move(old_index, new_index);
    }

    public void RemoveCondition(Parameter p) {
        for (int i = _conditions.Count - 1; i >= 0; i--) {
            if (_conditions[i].parameter.name == p.name) _conditions.RemoveAt(i);
        }
    }
}


/// <summary>
/// Class defining Condition
/// Contains a parameter and a bool that defines
/// whether the parameter should bet true or false
/// </summary>
[System.Serializable]
public class Condition {
    [SerializeField] Parameter _parameter;
    [SerializeField] bool _should_parameter_be_true;

    public Parameter parameter {
        get { return _parameter; }
        set { _parameter = value; }
    }

    public bool should_parameter_be_true {
        get { return _should_parameter_be_true; }
        set { _should_parameter_be_true = value; }
    }

    public Condition() {
        parameter = new Parameter("");
    }

    public Condition(Parameter parameter) {
        this.parameter = parameter;
    }

    public bool IsConditionMet(bool is_parameter_true) {
        return is_parameter_true == _should_parameter_be_true;
    }
}