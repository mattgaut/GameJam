using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditor : EditorWindow {

    Level to_edit;

    Tile to_place;
    Tile temp_tile;

    Vector2 mouse_position;
    Vector2Int tile_position;

    bool placing = false;
    bool deleting = false;

    bool is_mouse_over_scene;

    [MenuItem("Window/LevelEditor")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        LevelEditor window = (LevelEditor)GetWindow(typeof(LevelEditor));
        window.Show();
    }

    private void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }
    private void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        if (temp_tile != null) {
            DestroyImmediate(temp_tile.gameObject);
        }
    }

    void OnSceneGUI(SceneView scene_view) {
        mouse_position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        tile_position = Vector2Int.RoundToInt(mouse_position);

        HandleInputEvents();

        is_mouse_over_scene = mouseOverWindow == SceneView.currentDrawingSceneView;
        if (is_mouse_over_scene) {
            if (temp_tile == null && to_place != null) {
                temp_tile = Instantiate(to_place);
            }

            if (temp_tile != null) {
                temp_tile.transform.position = (Vector2)tile_position;
                temp_tile.gameObject.SetActive(!deleting);
                if (placing && !deleting) {
                    if (to_edit && !to_edit.HasTile(tile_position)) {
                        Tile tile = (Tile)PrefabUtility.InstantiatePrefab(to_place);
                        tile.transform.position = (Vector2)tile_position;
                        tile.transform.SetParent(to_edit.transform);
                        to_edit.TryAddTile(tile, tile_position);
                        Selection.objects = new Object[] { tile.gameObject };
                        Tools.current = Tool.None;

                        EditorUtility.SetDirty(to_edit);
                        EditorUtility.SetDirty(tile);
                    }
                } else if (placing && deleting) {
                    if (to_edit) {
                        Tile to_delete = to_edit.RemoveTile(tile_position);

                        if (to_delete) DestroyImmediate(to_delete.gameObject);

                        EditorUtility.SetDirty(to_edit);
                    }
                }
            }
        }

    }

    void HandleInputEvents() {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
            placing = true;
            GUIUtility.hotControl = 0;
            Event.current.Use();
        }
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
            placing = false;
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl) {
            deleting = true;
            GUIUtility.hotControl = 0;
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl) {
            deleting = false;
            Event.current.Use();
        }
    }

    private void OnGUI() {
        Level old_level = to_edit;
        to_edit = (Level)EditorGUILayout.ObjectField("Level To Edit", to_edit, typeof(Level), true);
        if ((old_level == null && to_edit != null) || to_edit != old_level) {
            to_edit.Init();
        }
        to_place = (Tile)EditorGUILayout.ObjectField("Tile To Place", to_place, typeof(Tile), false);
    }
}
