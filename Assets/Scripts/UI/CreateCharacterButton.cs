using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreateCharacterButton : MonoBehaviour
{
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
    public string colorType;
    public Color color;
    public Image btn;

    public bool isChecked = false;
    // Use this for initialization
    void Start()
    {
        if (isItem)
        {
            buttons = this.transform.parent.getAllChildren().getComponent<CreateCharacterButton>();
            btn = this.gameObject.GetComponent<Image>();
            Debug.Log("itemID: " + itemID);
            this.btn.sprite = itemID.getSprite();
            this.btn.preserveAspect = true;
            this.characterUI.addButton(this);
        }
        else if(isColor)
        {
            buttons = this.transform.parent.getAllChildren().getComponent<CreateCharacterButton>();
            btn = this.gameObject.GetComponent<Image>();
            this.btn.color = color;
            this.characterUI.addButton(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void onGoBack() {
        characterUI.getLogin().camera.panTo(new Vector3(12, 1.5f, 33f), Quaternion.Euler(0, -50, 0), 2f, 3f);
        characterUI.slideOut();
    }
    public void onClick()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].btn.color = new Color(buttons[i].color.r / 2, buttons[i].color.g / 2, buttons[i].color.b / 2);
            if (isItem)
            {
                buttons[i].isChecked = false;
                buttons[i].btn.color = new Color(1, 1, 1, 0.5f);
            }
        }
        this.btn.color = color;
        this.isChecked = true;
        if (isItem)
        {
            this.btn.color = new Color(1, 1, 1, 1);
            characterUI.equipItem(itemID, itemType);
        }
        else
        {
            characterUI.changeColor(this.btn.color, colorType);
        }
    }
}
