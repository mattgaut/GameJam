using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour {

    [SerializeField] Image item_image, border_image;
    [SerializeField] Text text;
    public Item.Type type { get; private set; }

    public void SetItem(Item i) {
        item_image.sprite = i.sprite;
        type = i.type;
    }

    public void SetCount(int count) {
        text.text = "x" + count;
    }

    public void SetHiglighted(bool highlight) {
        border_image.enabled = highlight;
    }
}
