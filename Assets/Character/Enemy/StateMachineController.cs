using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Class that ties a given state machine to callbacks in order to automatically execute behaviours
/// Parameter callbacks are used to inform state machine of the current parameter values
/// State coroutines are used to tie a particular state to a particular Coroutine
/// </summary>
public class StateMachineController : MonoBehaviour {

    [SerializeField] StateMachine state_machine;
    [SerializeField] List<ParameterCallback> parameter_callback_list;
    [SerializeField] List<StateCoroutine> state_coroutine_list;

    StateMachine.Instance state_machine_instance;

    Dictionary<State, Func<IEnumerator>> state_coroutines;
    Dictionary<Parameter, Func<bool>> parameter_callbacks;

    protected Coroutine state_machine_routine;

    public bool active { get; private set; }
    public virtual bool can_transition { get { return true; } }
    [NonSerialized] public bool should_disable_go_on_deactivate = true;

    /// <summary>
    /// Sets whether the behaviour should be running or not
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active) {
        if (active && !this.active) {
            Activate();
        } else if (!active) {
            Deactivate();
        }
    }

    protected virtual void Awake() {
        state_machine_instance = state_machine.GetStateMachineInstance();

        parameter_callbacks = new Dictionary<Parameter, Func<bool>>();
        foreach (ParameterCallback pc in parameter_callback_list) {
            pc.Init(this);
            parameter_callbacks.Add(pc.GetParameter(), pc.callback);
        }
        state_machine_instance.SetCallbacks(parameter_callbacks);

        state_coroutines = new Dictionary<State, Func<IEnumerator>>();
        foreach (StateCoroutine sc in state_coroutine_list) {
            sc.Init(this);
            state_coroutines.Add(sc.GetState(), sc.coroutine);
        }
    }


    /// <summary>
    /// While the routine is active yields to the routine tied to the current state of the state machine
    /// then once control is returned transitions to a new state if possible and continues again
    /// </summary>
    /// <returns></returns>
    protected IEnumerator StateMachineCoroutine() {
        while (active) {
            if (state_coroutines.ContainsKey(state_machine_instance.current_state)) {
                yield return state_coroutines[state_machine_instance.current_state].Invoke();
                while (!can_transition) {
                    yield return new WaitForFixedUpdate();
                }
                state_machine_instance.TransitionUsingCallbacks();
                //Debug.Log("Transition to " + state_machine_instance.current_state.name);
            } else {
                Debug.LogError("No Routine exists for state "  + state_machine_instance.current_state.name);
                break;
            }
        }
    }

    protected virtual void Activate() {
        active = true;
        gameObject.SetActive(true);
        state_machine_instance = state_machine.GetStateMachineInstance();
        state_machine_instance.SetCallbacks(parameter_callbacks);

        state_machine_routine = StartCoroutine(StateMachineCoroutine());
    }
    protected virtual void Deactivate() {
        active = false;        
        StopAllCoroutines();
        if (should_disable_go_on_deactivate) gameObject.SetActive(false);
    }

    /// <summary>
    /// Class that defines a parameter callback
    /// uses reflection to tie a particular parameter to a given boolean function in the
    /// StateMachineController class or child class
    /// </summary>
    [Serializable]
    class ParameterCallback {
        [SerializeField] Parameter parameter;
        [SerializeField] string callback_name;
        [SerializeField] bool callback_is_field;

        [SerializeField] int callback_index; // used by editor class

        public Func<bool> callback { get; private set; }

        /// <summary>
        /// Initializes callback variable by using reflection to find a bool method that takes no arguments
        /// in supplied object class or child that has the name stored in callback_name
        /// </summary>
        /// <param name="grab_from"></param>
        public void Init(object grab_from) {
            if (callback_is_field) {
                callback = () => (bool)grab_from.GetType().GetField(callback_name).GetValue(grab_from);
            } else {
                callback = Delegate.CreateDelegate(typeof(Func<bool>), grab_from, grab_from.GetType().GetMethod(callback_name)) as Func<bool>;
            }
        }

        public Parameter GetParameter() {
            return parameter;
        }
    }

    /// <summary>
    /// Class that degines a StateCoroutine
    /// Uses reflection to tie a particular State to a IEnumerator
    /// that exists in the StateMachineController class or child class
    /// </summary>
    [Serializable]
    class StateCoroutine {
        [SerializeField] State state;
        [SerializeField] string coroutine_name;

        [SerializeField] int coroutine_index; // used by editor class 

        public Func<IEnumerator> coroutine { get; private set; }

        /// <summary>
        /// Initializes corutine variable by using reflection to find a IEnumerator method that takes no arguments
        /// in supplied object class or child that has the name stored in coroutine_name
        /// </summary>
        /// <param name="grab_from"></param>
        public void Init(System.Object grab_from) {
            MethodInfo mi = grab_from.GetType().GetMethod(coroutine_name, BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[0], new ParameterModifier[0]);
            coroutine = Delegate.CreateDelegate(typeof(Func<IEnumerator>), grab_from, mi) as Func<IEnumerator>;
        }

        public State GetState() {
            return state;
        }
    }
}