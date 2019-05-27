using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour {

    [SerializeField] InventoryButton prefab;
    [SerializeField] Dictionary<Item.Type, InventoryButton> buttons;
    List<InventoryButton> button_list;

    [SerializeField] Transform parent_transform;

    [SerializeField] Character character;

    int button_selected = 0;

    private void Awake() {
        buttons = new Dictionary<Item.Type, InventoryButton>();
        button_list = new List<InventoryButton>();
    }

    private void Start() {
        character.inventory.on_add += AddItem;

        character.inventory.on_remove += RemoveItem;
    }

    private void Update() {
        if (button_selected >= button_list.Count) {
            SelectButton(button_list.Count - 1);
        }
        if (Input.GetButtonDown("PlaceItem") && button_list.Count > 0) {
            Item i = character.inventory.TryRemoveItem(button_list[button_selected].type);
            if (i != null) {
                i.SetSpriteEnabled(true);
                i.transform.SetParent(SceneManager.GetActiveScene().GetRootGameObjects()[0].transform);
                i.transform.SetParent(null);
                i.transform.position = character.transform.position;
                i.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
        if (Input.GetButtonDown("SwapInventory")) {
            SelectButton((int)Mathf.Clamp(button_selected + Input.GetAxisRaw("SwapInventory"), 0, button_list.Count));
        }
    }

    public void SelectButton(int button) {
        button_selected = button;
        for (int i = 0; i < button_list.Count; i++) {
            button_list[i].SetHiglighted(false);
        }
        if (button_list.Count > button_selected && button_selected != -1) {
            button_list[button_selected].SetHiglighted(true);
        }
    }

    void AddItem(Item i) {
        if (buttons.ContainsKey(i.type)) {
            buttons[i.type].SetCount(character.inventory.ItemCount(i.type));
        } else {
            InventoryButton new_button = Instantiate(prefab);
            new_button.transform.SetParent(parent_transform);

            new_button.SetItem(i);
            new_button.SetCount(1);

            button_list.Add(new_button);

            SelectButton(button_list.Count -1);

            buttons.Add(i.type, new_button);
        }
    }

    void RemoveItem(Item.Type i) {
        if (buttons.ContainsKey(i)) {
            int count = character.inventory.ItemCount(i);
            if (count == 0) {
                button_list.Remove(buttons[i]);
                Destroy(buttons[i].gameObject);
                buttons.Remove(i);
            } else {
                buttons[i].SetCount(count);
            }
        }
    }

}
