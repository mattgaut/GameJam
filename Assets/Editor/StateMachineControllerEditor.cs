using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

[CustomEditor(typeof(StateMachineController), true)]
public class StateMachineControllerEditor : Editor {

    bool parameters_foldout = false, states_foldout = false;
    bool states_matched = false, arrays_matched = false;

    MemberInfo[] members;

    List<string> state_candidate_names;
    List<string> parameter_candidate_names;

    SerializedObject serialized_target;
    SerializedProperty state_machine_property;

    private void OnEnable() {
        serialized_target = new SerializedObject(target);
        state_machine_property = serialized_target.FindProperty("state_machine");

        members = GetStateCandidates(target.GetType());
        state_candidate_names = new List<string>();
        for (int i = 0; i < members.Length; i++) {
            state_candidate_names.Insert(i, members[i].Name);
        }

        members = GetParameterCandidates(target.GetType());
        parameter_candidate_names = new List<string>(members.Length);
        for (int i = 0; i < members.Length; i++) {
            parameter_candidate_names.Insert(i, members[i].Name);
        }

        if (state_machine_property.objectReferenceValue != null) {
            UpdateStateMachineAttachments();
        }
    }

    public override void OnInspectorGUI() {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        GUI.enabled = true;

        System.Object old_object = state_machine_property.objectReferenceValue;
        state_machine_property.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("State Machine"), state_machine_property.objectReferenceValue, typeof(StateMachine), false);

        if (!ReferenceEquals(state_machine_property.objectReferenceValue, old_object) && state_machine_property.objectReferenceValue != null) {
            ClearThenUpdateAttachments();
        }

        if (state_machine_property.objectReferenceValue != null) {
            SerializedProperty parameters_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("parameters");
            SerializedProperty callback_list_property = serialized_target.FindProperty("parameter_callback_list");

            if (callback_list_property.arraySize != parameters_property.arraySize) {
                callback_list_property.ClearArray();
                for (int i = 0; i < parameters_property.arraySize; i++) {
                    callback_list_property.InsertArrayElementAtIndex(i);
                }
            }

            parameters_foldout = EditorGUILayout.Foldout(parameters_foldout, "Parameters");
            if (parameters_foldout) {
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < callback_list_property.arraySize; i++) {
                    SerializedProperty array_element = callback_list_property.GetArrayElementAtIndex(i);
                    string parameter_name = parameters_property.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
                    SerializedProperty index = array_element.FindPropertyRelative("callback_index");

                    index.intValue = EditorGUILayout.Popup(parameter_name, index.intValue, parameter_candidate_names.ToArray());

                    if (members.Length > index.intValue) {
                        array_element.FindPropertyRelative("callback_is_field").boolValue = (members[index.intValue].MemberType == MemberTypes.Field);
                        array_element.FindPropertyRelative("callback_name").stringValue = members[index.intValue].Name;
                        array_element.FindPropertyRelative("parameter").FindPropertyRelative("_name").stringValue = parameter_name;
                    }
                }
                EditorGUI.indentLevel -= 1;
            }

            SerializedProperty states_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("states");
            SerializedProperty coroutine_list_property = serialized_target.FindProperty("state_coroutine_list");

            states_foldout = EditorGUILayout.Foldout(states_foldout, "States");
            if (states_foldout) {
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < coroutine_list_property.arraySize; i++) {
                    SerializedProperty array_element = coroutine_list_property.GetArrayElementAtIndex(i);
                    string state_name = states_property.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
                    SerializedProperty index = array_element.FindPropertyRelative("coroutine_index");

                    index.intValue = EditorGUILayout.Popup(state_name, index.intValue, state_candidate_names.ToArray());

                    if (state_candidate_names.Count > index.intValue) {
                        array_element.FindPropertyRelative("coroutine_name").stringValue = state_candidate_names[index.intValue];
                        array_element.FindPropertyRelative("state").FindPropertyRelative("_name").stringValue = state_name;
                    }
                }
                EditorGUI.indentLevel -= 1;
            }
        }

        EditorGUILayout.Space();

        DrawPropertiesExcluding(serialized_target, new string[] { "state_machine", "parameter_callback_list", "state_coroutine_list", "m_Script" });

        serialized_target.ApplyModifiedProperties();
    }

    MemberInfo[] GetParameterCandidates(System.Type type) {
        List<MemberInfo> member_info_list = new List<MemberInfo>();

        member_info_list.AddRange(type.FindMembers(MemberTypes.Method | MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public, (a, b) => MatchParameterCandidate(a, b, type), null));

        return member_info_list.ToArray();
    }

    bool MatchParameterCandidate(MemberInfo member, object filter, System.Type type) {
        if (member.MemberType == MemberTypes.Method) {
            MethodInfo mi = type.GetMethod(member.Name, new System.Type[0]);
            if (typeof(StateMachineController).BaseType.GetMethod(member.Name, new System.Type[0]) != null) {
                return false;
            }

            return mi != null && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(bool);
        }
        if (member.MemberType == MemberTypes.Field) {
            return (member as FieldInfo).FieldType == typeof(bool);
        }
        return false;
    }

    MemberInfo[] GetStateCandidates(System.Type type) {
        List<MemberInfo> member_info_list = new List<MemberInfo>();
        member_info_list.AddRange(type.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.NonPublic, (a, b) => MatchStateCandidate(a, b, type), null));

        return member_info_list.ToArray();
    }

    bool MatchStateCandidate(MemberInfo member, object filter, System.Type type) {
        if (member.MemberType == MemberTypes.Method && member.Name != "StateMachineCoroutine") {

            MethodInfo mi = type.GetMethod(member.Name, BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[0], new ParameterModifier[0]);            

            return mi != null && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(IEnumerator);
        }
        return false;
    }

    void ClearThenUpdateAttachments() {
        SerializedProperty parameters_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("parameters");
        SerializedProperty callback_list_property = serialized_target.FindProperty("parameter_callback_list");
        callback_list_property.ClearArray();

        UpdateParameters(callback_list_property, parameters_property, parameter_candidate_names);

        SerializedProperty states_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("states");
        SerializedProperty coroutine_list_property = serialized_target.FindProperty("state_coroutine_list");
        coroutine_list_property.ClearArray();

        UpdateStates(coroutine_list_property, states_property, state_candidate_names);
    }

    void UpdateStateMachineAttachments() {
        SerializedProperty parameters_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("parameters");
        SerializedProperty callback_list_property = serialized_target.FindProperty("parameter_callback_list");

        UpdateParameters(callback_list_property, parameters_property, parameter_candidate_names);

        SerializedProperty states_property = new SerializedObject(state_machine_property.objectReferenceValue).FindProperty("states");
        SerializedProperty coroutine_list_property = serialized_target.FindProperty("state_coroutine_list");

        UpdateStates(coroutine_list_property, states_property, state_candidate_names);
    }

    void UpdateStates(SerializedProperty coroutine_list_property, SerializedProperty states_property, List<string> state_candidate_names) {
        Dictionary<string, StateCoroutineInfo> routines = new Dictionary<string, StateCoroutineInfo>();

        for (int i = 0; i < coroutine_list_property.arraySize; i++) {
            string element_name = coroutine_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("state").FindPropertyRelative("_name").stringValue;
            routines.Add(element_name, new StateCoroutineInfo());

            routines[element_name].state = element_name;
            routines[element_name].routine = coroutine_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("coroutine_name").stringValue;
            routines[element_name].index = coroutine_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("coroutine_index").intValue;
        }
        coroutine_list_property.ClearArray();

        for (int i = 0; i < states_property.arraySize; i++) {
            coroutine_list_property.InsertArrayElementAtIndex(i);

            SerializedProperty new_element = coroutine_list_property.GetArrayElementAtIndex(i);
            string state_name = states_property.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
            new_element.FindPropertyRelative("state").FindPropertyRelative("_name").stringValue = state_name;

            if (routines.ContainsKey(state_name) && state_candidate_names.Contains(routines[state_name].routine)) {
                new_element.FindPropertyRelative("coroutine_name").stringValue = routines[state_name].routine;
                new_element.FindPropertyRelative("coroutine_index").intValue = state_candidate_names.IndexOf(routines[state_name].routine);
            }
        }
    }

    void UpdateParameters(SerializedProperty callback_list_property, SerializedProperty parameters, List<string> parameter_candidate_names) {
        Dictionary<string, ParameterCallbackInfo> callbacks = new Dictionary<string, ParameterCallbackInfo>();

        for (int i = 0; i < callback_list_property.arraySize; i++) {
            string element_name = callback_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("parameter").FindPropertyRelative("_name").stringValue;
            callbacks.Add(element_name, new ParameterCallbackInfo());

            callbacks[element_name].parameter = element_name;
            callbacks[element_name].callback_name = callback_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("callback_name").stringValue;
            callbacks[element_name].callback_index = callback_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("callback_index").intValue;
            callbacks[element_name].callback_is_field = callback_list_property.GetArrayElementAtIndex(i).FindPropertyRelative("callback_is_field").boolValue;

        }
        callback_list_property.ClearArray();

        for (int i = 0; i < parameters.arraySize; i++) {
            callback_list_property.InsertArrayElementAtIndex(i);

            SerializedProperty new_element = callback_list_property.GetArrayElementAtIndex(i);
            string parameter_name = parameters.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
            new_element.FindPropertyRelative("parameter").FindPropertyRelative("_name").stringValue = parameter_name;

            if (callbacks.ContainsKey(parameter_name) && parameter_candidate_names.Contains(callbacks[parameter_name].callback_name)) {
                new_element.FindPropertyRelative("callback_name").stringValue = callbacks[parameter_name].callback_name;
                new_element.FindPropertyRelative("callback_index").intValue = parameter_candidate_names.IndexOf(callbacks[parameter_name].callback_name);
                new_element.FindPropertyRelative("callback_is_field").boolValue = callbacks[parameter_name].callback_is_field;
            }
        }
    }

    class StateCoroutineInfo {
        public string state;
        public string routine;
        public int index;
    }

    class ParameterCallbackInfo {
        public string parameter;
        public string callback_name;
        public int callback_index;
        public bool callback_is_field;
    }
}
