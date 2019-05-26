using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    [SerializeField] List<Tile> tiles;

    Dictionary<Vector2Int, Tile> tile_dict;

    public void Init() {
        tile_dict = new Dictionary<Vector2Int, Tile>();

        foreach (Tile t in tiles) {
            tile_dict.Add(t.position, t);
        }
    }

    public bool HasTile(Vector2Int pos) {
        return tile_dict.ContainsKey(pos);
    }

    public bool TryAddTile(Tile tile, Vector2Int position) {
        if (HasTile(position)) {
            return false;
        }
        tile.SetPosition(position);
        tiles.Add(tile);
        tile_dict.Add(position, tile);
        return true;
    }

    public Tile GetTile(Vector2Int position) {
        if (HasTile(position)) {
            return null;
        }
        return tile_dict[position];
    }

    public Tile RemoveTile(Vector2Int position) {
        if (!HasTile(position)) {
            return null;
        }
        Tile tile = tile_dict[position];
        tiles.Remove(tile);
        tile_dict.Remove(position);
        return tile;
    }
}
