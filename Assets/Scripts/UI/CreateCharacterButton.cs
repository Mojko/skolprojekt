using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreateCharacterButton : MonoBehaviour {
    public CreateCharacterButton[] buttons;
    public CreateCharacterUI characterUI;
    [Header("Item")]
    [Space(2)]
    public bool isItem = false;
    public string itemType;
    public int itemID;
    [Space(10)]
    [Header("Color")]
    [Space(2)]
    public bool isColor = false;
    public Color color;
    public Image btn;
	// Use this for initialization
	void Start () {
        buttons = this.transform.parent.getAllChildren().getComponent<CreateCharacterButton>();
        Debug.Log("children lengfth: " + buttons.Length);
        btn = this.gameObject.GetComponent<Image>();
        if (isItem)
        {
            this.btn.sprite = ItemDataProvider.getInstance().getStats(itemID).getInt("imageIndex").getSprite();
        }
        else {
            this.btn.color = color;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onClick(){
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].btn.color = new Color(buttons[i].color.r / 2, buttons[i].color.g / 2, buttons[i].color.b / 2);
        }
        this.btn.color = color;
        if (isItem) {
            characterUI.equipItem(itemID, itemType);
        }
    }
}
