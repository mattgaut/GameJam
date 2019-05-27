using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TameShopButton : MonoBehaviour {
    [SerializeField] Text item_name, price, count;
    [SerializeField] Image item_image;
    [SerializeField] Button _button;
    
    public Button button {
        get { return _button; }
    }

    public void Set(string name, int price, Sprite image, int count) {
        item_name.text = name;
        this.price.text = price + " Gold";
        item_image.sprite = image;
        this.count.text = "x" + count;
    }
}
