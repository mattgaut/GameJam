using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class LevelEditor : EditorWindow {

    Level to_edit;

    Tile to_place;
    Tile temp_tile;

    Vector2 mouse_position;
    Vector2Int tile_position;

    bool placing = false;
    bool deleting = false;

    bool is_mouse_over_scene;

    bool can_delete_background;

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
            if (to_place != null && (temp_tile == null || temp_tile.name != to_place.name)) {
                if (temp_tile != null) DestroyImmediate(temp_tile);
                temp_tile = Instantiate(to_place);
                temp_tile.name = to_place.name;
            }

            if (temp_tile != null) {
                temp_tile.transform.position = (Vector2)tile_position;
                temp_tile.gameObject.SetActive(!deleting);
                if (placing && !deleting) {
                    if (to_edit && ((!to_place.is_background_tile && !to_edit.HasTile(tile_position)) || (to_place.is_background_tile && !to_edit.HasBackgroundTile(tile_position)))) {
                        Tile tile = (Tile)PrefabUtility.InstantiatePrefab(to_place);
                        tile.transform.rotation = temp_tile.transform.rotation;
                        tile.transform.position = (Vector2)tile_position;
                        tile.transform.SetParent(to_edit.transform);
                        if (tile.is_background_tile) {
                            to_edit.TryAddBackgroundTile(tile, tile_position);
                        } else {
                            to_edit.TryAddTile(tile, tile_position);
                        }
                        Selection.objects = new Object[] { tile.gameObject };
                        Tools.current = Tool.None;

                        EditorUtility.SetDirty(to_edit);
                        EditorUtility.SetDirty(tile);
                    }
                } else if (placing && deleting) {
                    if (to_edit) {
                        Tile to_delete = null;
                        if (can_delete_background) {
                            to_delete = to_edit.RemoveBackgroundTile(tile_position);
                            if (to_delete) DestroyImmediate(to_delete.gameObject);
                        }

                        to_delete = to_edit.RemoveTile(tile_position);
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
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F) {
            temp_tile.transform.rotation *= Quaternion.Euler(0,180, 0);
            GUIUtility.hotControl = 0;
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

        can_delete_background = EditorGUILayout.Toggle(can_delete_background);
    }
}
#endif