using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public static class EditorExtensions {
    public static T GetCurrent<T>(this ReorderableList list) where T : class {
        if (list.index < 0 || list.index >= list.count) {
            return null;
        } else {
            return list.list[list.index] as T;
        }
    }

    public static T Get<T>(this ReorderableList list, int index) where T : class {
        if (index < 0 || index >= list.count) {
            return null;
        } else {
            return list.list[index] as T;
        }
    }
}
