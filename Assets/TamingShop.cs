using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TamingShop : MonoBehaviour {

    [SerializeField] List<ShopItem> items;

    [SerializeField] Canvas shop_screen;
    [SerializeField] Text player_gold;
    [SerializeField] RectTransform shop_transform;
    [SerializeField] TameShopButton button_prefab;

    public bool TrySellItem(ShopItem i) {
        if (i.count > 0 && GameManager.instance.player.TrySpendCoins(i.price)) {
            i.count--;
            Item item = Instantiate(i.item);
            GameManager.instance.player.inventory.AddItem(item);
            item.transform.SetParent(GameManager.instance.player.transform);
            item.GetComponent<SpriteRenderer>().enabled = false;
            player_gold.text = GameManager.instance.player.CoinCount() + " Gold Remaining";
            return true;
        }
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
            if (Input.GetButtonDown("Interact")) {
                OpenShop();
            }
            if (Input.GetButtonDown("Attack")) {
                CloseShop();
            }
        }
    }

    public void CloseShop() {
        Invoke(nameof(CloseTheShop), 0.02f);
    }

    void CloseTheShop() {
        GameManager.instance.player.GetComponent<PlayerInputHandler>().enabled = true;
        shop_screen.gameObject.SetActive(false);
    }

    void OpenShop() {
        shop_screen.gameObject.SetActive(true);

        GameManager.instance.player.GetComponent<PlayerInputHandler>().enabled = false;
        
        player_gold.text = GameManager.instance.player.CoinCount() + " Gold Remaining";
        for (int i = shop_transform.childCount - 1; i >= 0; i--) {
            Destroy(shop_transform.GetChild(i).gameObject);
        }

        foreach (ShopItem si in items) {
            ShopItem shop_item = si;
            TameShopButton new_shop_button = Instantiate(button_prefab);
            new_shop_button.transform.SetParent(shop_transform);
            new_shop_button.transform.SetAsFirstSibling();
            EventSystem.current.SetSelectedGameObject(new_shop_button.gameObject);
            UpdateButton(new_shop_button, shop_item);
            new_shop_button.button.onClick.AddListener(() => { if (TrySellItem(shop_item)) UpdateButton(new_shop_button, shop_item); });
        }        
    }

    private void Update() {
        if (shop_screen.isActiveAndEnabled) {
            if (EventSystem.current.currentSelectedGameObject == null && shop_transform.childCount > 0) {
                EventSystem.current.SetSelectedGameObject(shop_transform.GetChild(0).gameObject);
            }
        }
    }

    void UpdateButton(TameShopButton button, ShopItem si) {
        button.Set(nameof(si.item), si.price, si.sprite, si.count);
        if (si.count == 0) {
            Destroy(button.gameObject);
        }
    }

    [System.Serializable]
    public class ShopItem {
        public Item item;
        public Sprite sprite;
        public int price;
        public int count;
    }
}
