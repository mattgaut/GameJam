using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public static class Extension {

    /// <summary>
    /// Shuffles a list using the Knuth\Fisher-Yates shuffle method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(this List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    /// <summary>
    /// Gets a random element of a list
    /// List must not be empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandom<T>(this List<T> list) {
        if (list.Count == 0) throw new System.Exception("Trying to get random number from empty list");
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Gets a random element of a collection
    /// collection must not be empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandom<T>(this ICollection<T> collection) {
        int index = Random.Range(0, collection.Count);
        foreach (T item in collection) { if (index-- == 0) return item; }
        throw new System.Exception("Trying to get random number from empty collection");
    }

    /// <summary>
    /// Moves object from one index to another
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="index_from"></param>
    /// <param name="index_to"></param>
    /// <returns>bool that corresponds to whether the move was performed</returns>
    public static bool Move<T>(this List<T> list, int index_from, int index_to) {
        if (list.Count <= index_from) {
            return false;
        }

        T to_move = list[index_from];
        list.RemoveAt(index_from);
        list.Insert(index_to, to_move);

        return true;
    }

    /// <summary>
    /// Get IEnumerable that iterates through the cardinal directions adjacent to vector
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>IEnumerable of Vector2Ints adjacent to vector</returns>
    public static IEnumerable<Vector2Int> GetNeighbors(this Vector2Int vector) {
        yield return vector + Vector2Int.up;
        yield return vector + Vector2Int.right;
        yield return vector + Vector2Int.down;
        yield return vector + Vector2Int.left;
    }
}