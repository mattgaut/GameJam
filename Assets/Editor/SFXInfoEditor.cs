using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SFXInfo))]
public class SFXInfoPropertyDrawer : PropertyDrawer {

    float height = 0;
    NamePopupWindow active_popup_window;

    string selected_name = "";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;
        Vector2 shift_vector = (rect.height + 2) * Vector2.up;

        bool folded_out = EditorGUI.PropertyField(rect, property, false);
        rect.position += shift_vector;

        if (folded_out) {
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(rect, property.FindPropertyRelative("_override_clip"));
            rect.position += shift_vector;

            SerializedProperty default_name = property.FindPropertyRelative("_default_codename");

            GUI.SetNextControlName("Namer");
            string typed_value = EditorGUI.TextField(rect, new GUIContent("Default Codename"), default_name.stringValue);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) {
                active_popup_window = new NamePopupWindow(new Vector2(position.width, 300), property.name, SetDefaultName);
                PopupWindow.Show(rect, active_popup_window);
            }

            rect.position += shift_vector;

            if (selected_name != "") {
                default_name.stringValue = selected_name;
                GUI.FocusControl(null);
                EditorGUI.FocusTextInControl(null);
                selected_name = "";
            } else {
                default_name.stringValue = typed_value;
            }

            EditorGUI.indentLevel--;
        }

        height = rect.y - position.y;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return height;
    }

    void SetDefaultName(string new_name) {
        selected_name = new_name;
        active_popup_window = null;
    }
}

public class NamePopupWindow : PopupWindowContent {

    Vector2 size;
    string search_string;

    Action<string> on_click;

    string search_name;

    public NamePopupWindow(Vector2 size, string search_name, Action<string> action) {
        this.size = size;
        on_click = action;
        this.search_name = search_name;
        search_string = "";
    }

    public override Vector2 GetWindowSize() {
        return size;
    }

    public override void OnGUI(Rect rect) {
        search_string = EditorGUILayout.TextField(search_name +": ", search_string);
        SoundBank bank = AssetDatabase.LoadAssetAtPath<SoundBank>("Assets/SFX/SoundBank/SoundBank.asset");
        string[] codenames = bank.GetAllSFXClipCodenames().ToArray();
        string[] names = bank.GetAllSFXClipNames().ToArray();
        for (int i = 0; i < codenames.Length; i++) {
            if (search_string != "" && !codenames[i].Contains(search_string) && !names[i].Contains(search_string)) {
                continue;
            }
            if (GUILayout.Button(names[i] + " : " + codenames[i])) {
                on_click(codenames[i]);
                editorWindow.Close();
            }
        }
    }
}

