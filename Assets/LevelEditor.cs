using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour {

    [SerializeField] Tile to_place;

    Tile temp_tile;

    Vector2 mouse_position;
    Vector2Int tile_position;

    private void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }
    private void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        if (temp_tile) {
            DestroyImmediate(temp_tile);
        }
    }

    void OnSceneGUI(SceneView scene_view) {
        mouse_position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        tile_position = Vector2Int.RoundToInt(mouse_position);

        if (temp_tile == null && to_place != null) {
            temp_tile = Instantiate(to_place);
        }

        if (temp_tile != null) {
            temp_tile.transform.position = (Vector2)tile_position;
        }
    }
}
