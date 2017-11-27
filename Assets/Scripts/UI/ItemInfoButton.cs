using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemInfoButton : MonoBehaviour {
    private ItemInfoHandler handler;
    private Button button;
	// Use this for initialization
	void Start () {
        handler = this.transform.parent.gameObject.GetComponent<ItemInfoHandler>();
        button = this.GetComponent<Button>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void changeButtonClick(string message, int method) {
        switch (method)
        {
            case 0:
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onEquip());
                break;
            case 1:
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onUse());
                break;
            case 2:

                break;
        }
        button.transform.GetChild(0).GetComponent<Text>().text = message;
    }
    public void onEquip() {
        handler.setEquipClicked();
    }
    public void onDrop() {

    }
    public void onUse() {
        handler.setUseClicked();
    }
}
