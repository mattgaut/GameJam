using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    [SerializeField] List<Tile> tiles;
    [SerializeField] List<Tile> background_tiles;

    Dictionary<Vector2Int, Tile> tile_dict;
    Dictionary<Vector2Int, Tile> background_tile_dict;

    public void Init() {
        tile_dict = new Dictionary<Vector2Int, Tile>();

        background_tile_dict = new Dictionary<Vector2Int, Tile>();

        foreach (Tile t in tiles) {
            tile_dict.Add(t.position, t);
        }
        foreach (Tile t in background_tiles) {
            background_tile_dict.Add(t.position, t);
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

    public bool HasBackgroundTile(Vector2Int pos) {
        return background_tile_dict.ContainsKey(pos);
    }

    public bool TryAddBackgroundTile(Tile tile, Vector2Int position) {
        if (HasBackgroundTile(position)) {
            return false;
        }
        tile.SetPosition(position);
        background_tiles.Add(tile);
        background_tile_dict.Add(position, tile);
        return true;
    }

    public Tile GetBackgroundTile(Vector2Int position) {
        if (HasBackgroundTile(position)) {
            return null;
        }
        return background_tile_dict[position];
    }

    public Tile RemoveBackgroundTile(Vector2Int position) {
        if (!HasBackgroundTile(position)) {
            return null;
        }
        Tile tile = background_tile_dict[position];
        background_tiles.Remove(tile);
        background_tile_dict.Remove(position);
        return tile;
    }
}
