using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SFXClip))]
public class SFXClipEditor : Editor {

    static Scene sound_scene;
    static AudioSource sound_object;
    static bool scene_loaded;

    GUIStyle line_style;

    private void OnEnable() {
        line_style = new GUIStyle();
        line_style.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;

        if (!scene_loaded) {
            sound_scene = EditorSceneManager.NewPreviewScene();

            sound_object = new GameObject("Sound Test Object", typeof(AudioSource)).GetComponent<AudioSource>();

            SceneManager.MoveGameObjectToScene(sound_object.gameObject, sound_scene);

            scene_loaded = true;
        } 
    }

    private void OnDisable() {
        if (scene_loaded) {
            DestroyImmediate(sound_object.gameObject);

            EditorSceneManager.ClosePreviewScene(sound_scene);

            scene_loaded = false;
        }
    }

    public override void OnInspectorGUI() {

        SFXClip clip = target as SFXClip;

        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (scene_loaded) {
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 1);
            GUI.Box(rect, "", line_style);

            if (GUILayout.Button("Test Sound")) {
                TestSound(clip);
            }

            sound_object.volume = EditorGUILayout.Slider("Game Volume", sound_object.volume, 0, 1);
        }
    } 

    void TestSound(SFXClip clip) {
        if (sound_object != null) { clip.PlaySound(sound_object); } 
    }
}
