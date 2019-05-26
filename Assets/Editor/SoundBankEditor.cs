using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundBank))]
public class SoundBankEditor : Editor {
    GUIStyle error_style;
    GUIStyle line_style;

    bool is_create_clip_folded_out = true, is_add_clip_folded_out = false;
    AudioClip clip;
    string new_clip_name = "", new_clip_codename;
    SFXClip add_clip;

    string search_string = "";

    public override void OnInspectorGUI() {
        SoundBank bank = target as SoundBank;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(bank), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        search_string = EditorGUILayout.TextField(search_string);

        SerializedProperty array = serializedObject.FindProperty("_sound_effect_list");
        for (int i = 0; i < array.arraySize; i++) {
            SFXClip current_clip = array.GetArrayElementAtIndex(i).objectReferenceValue as SFXClip;
            if (current_clip == null || (search_string != "" && !current_clip.codename.Contains(search_string) && !current_clip.name.Contains(search_string))) {
                continue;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue != null) {
                    array.DeleteArrayElementAtIndex(i);
                }
                array.DeleteArrayElementAtIndex(i);
                i--;
                serializedObject.ApplyModifiedProperties();
                continue;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(array.GetArrayElementAtIndex(i).objectReferenceValue, typeof(SFXClip), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        is_create_clip_folded_out = DrawLine(is_create_clip_folded_out, "Create SFXClip");
        if (is_create_clip_folded_out) {
            EditorGUI.indentLevel += 1;
            CreateClip(bank);
            EditorGUI.indentLevel -= 1;
        }

        is_add_clip_folded_out = DrawLine(is_add_clip_folded_out, "Add SFXClip");
        if (is_add_clip_folded_out) {
            EditorGUI.indentLevel += 1;
            AddClip(bank);
            EditorGUI.indentLevel -= 1;
        }
    }

    void CreateAndAddToBank(SerializedObject bank, AudioClip clip, string name, string codename) {
        string path = "Assets/SFX/SoundBank/Resources";
        path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");
        SFXClip sfx_clip = CreateInstance<SFXClip>();
        SerializedObject sfx_clip_object = new SerializedObject(sfx_clip);
        sfx_clip_object.FindProperty("_codename").stringValue = codename;
        sfx_clip_object.FindProperty("_clip").objectReferenceValue = clip;
        sfx_clip_object.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(sfx_clip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SerializedProperty property = bank.FindProperty("_sound_effect_list");
        property.InsertArrayElementAtIndex(property.arraySize);
        property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = sfx_clip;

        bank.ApplyModifiedProperties();
    }

    void AddToBank(SerializedObject bank, SFXClip sfx_clip) {
        SerializedObject sfx_clip_object = new SerializedObject(sfx_clip);
        sfx_clip_object.FindProperty("_codename").stringValue = SoundBank.GetCodename(sfx_clip.name);
        sfx_clip_object.ApplyModifiedProperties();

        string new_path = "Assets/SFX/SoundBank/Resources/" + sfx_clip.name + ".asset";
        AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(sfx_clip), new_path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SerializedProperty property = bank.FindProperty("_sound_effect_list");
        property.InsertArrayElementAtIndex(property.arraySize);
        property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SFXClip>(new_path);

        bank.ApplyModifiedProperties();
    }

    bool DrawLine(bool folded_out, string label) {
        EditorGUILayout.Space();

        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 15);

        folded_out = EditorGUI.Foldout(rect, folded_out, new GUIContent(label), true);
        rect.height = 1;
        rect.y += 7;
        rect.x += 100;
        GUI.Box(rect, "", line_style);

        EditorGUILayout.Space();

        return folded_out;
    }

    void CreateClip(SoundBank bank) {
        new_clip_name = EditorGUILayout.TextField("New SFXClip Name", new_clip_name);
        new_clip_codename = SoundBank.GetCodename(new_clip_name);
        EditorGUILayout.LabelField("Codename", new_clip_codename);

        EditorGUILayout.BeginHorizontal();

        clip = (AudioClip)EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);

        EditorGUI.BeginDisabledGroup(clip == null || bank.HasSFXClip(new_clip_name) || new_clip_name == "");

        if (GUILayout.Button("Create New SFXClip")) {
            CreateAndAddToBank(serializedObject, clip, new_clip_name, new_clip_codename);
            clip = null;
            new_clip_name = "";
            new_clip_codename = "";
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (bank.HasSFXClip(new_clip_codename)) {
            EditorGUILayout.LabelField(new_clip_codename + " already exists in sound bank.", error_style);
        }
    }

    void AddClip(SoundBank bank) {
        EditorGUILayout.BeginHorizontal();

        add_clip = (SFXClip)EditorGUILayout.ObjectField(add_clip, typeof(SFXClip), false);

        EditorGUI.BeginDisabledGroup(add_clip == null || bank.HasSFXClip(SoundBank.GetCodename(add_clip.name)));

        if (GUILayout.Button("Add SFXClip")) {
            AddToBank(serializedObject, add_clip);
            add_clip = null;
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (add_clip != null && bank.HasSFXClip(SoundBank.GetCodename(add_clip.name))) {
            EditorGUILayout.LabelField(SoundBank.GetCodename(add_clip.name) + " already exists in sound bank.", error_style);
        }
    }

    private void Awake() {
        error_style = new GUIStyle(EditorStyles.label);
        error_style.normal.textColor = Color.red;

        line_style = new GUIStyle();
        line_style.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;
    }
}
