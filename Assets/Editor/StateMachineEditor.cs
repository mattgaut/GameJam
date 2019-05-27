using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StateMachine), true)]
public class StateMachineEditor : Editor {

    bool are_states_folded_out;
    bool are_transitions_folded_out;
    bool are_parameters_folded_out;

    public override void OnInspectorGUI() {

        StateMachine machine = target as StateMachine;
        SerializedObject serialized_machine = new SerializedObject(machine);
        if (GUILayout.Button("Edit")) {
            StateMachineEditorWindow.ShowWindow(machine);
        }

        GUI.enabled = false;


        SerializedProperty property = serialized_machine.FindProperty("states");
        DrawFoldoutList(ref are_states_folded_out, property, property.arraySize + " States");

        property = serialized_machine.FindProperty("transitions");
        DrawFoldoutList(ref are_transitions_folded_out, property, property.arraySize + " Transitions");

        property = serialized_machine.FindProperty("parameters");
        DrawFoldoutList(ref are_parameters_folded_out, property, property.arraySize + " Parameters");

        GUI.enabled = true;

        serialized_machine.ApplyModifiedProperties();
    }

    void DrawFoldoutList(ref bool is_folded_out, SerializedProperty array_property, string name) {
        is_folded_out = EditorGUILayout.Foldout(is_folded_out, name);

        EditorGUI.indentLevel += 1;
        if (is_folded_out) {
            for (int i = 0; i < array_property.arraySize; i++) {
                EditorGUILayout.PropertyField(array_property.GetArrayElementAtIndex(i));
            }
        }
        EditorGUI.indentLevel -= 1;
    }
}

[CustomPropertyDrawer(typeof(State))]
public class StatePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.Label(EditorGUI.IndentedRect(position), property.FindPropertyRelative("_name").stringValue);
    }
}

[CustomPropertyDrawer(typeof(Transition))]
public class TransitionPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.IndentedRect(position);

        GUI.Label(position, property.FindPropertyRelative("_from").FindPropertyRelative("_name").stringValue
            + " -> " + property.FindPropertyRelative("_to").FindPropertyRelative("_name").stringValue);

        position.y += EditorGUIUtility.singleLineHeight;

        SerializedProperty child_property = property.FindPropertyRelative("_conditions");
        child_property.isExpanded = EditorGUI.Foldout(position, child_property.isExpanded, child_property.arraySize + " Conditions");
        EditorGUI.indentLevel += 1;
        if (child_property.isExpanded) {
            for (int i = 0; i < child_property.arraySize; i++) {
                EditorGUILayout.PropertyField(child_property.GetArrayElementAtIndex(i));
            }
        }
        EditorGUI.indentLevel -= 1;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight * 2;
    }
}

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.IndentedRect(position);

        bool should_be_true = property.FindPropertyRelative("_should_parameter_be_true").boolValue;
        string paramerter_name = property.FindPropertyRelative("_parameter").FindPropertyRelative("_name").stringValue;
        GUI.Label(position, paramerter_name + " must be " + should_be_true.ToString());
    }
}

[CustomPropertyDrawer(typeof(Parameter))]
public class ParameterPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.IndentedRect(position);

        GUI.Label(position, property.FindPropertyRelative("_name").stringValue);
    }
}
