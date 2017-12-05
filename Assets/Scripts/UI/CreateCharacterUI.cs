using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreateCharacterUI : MonoBehaviour {
    bool shouldSlide = false;
    bool loaded = false;
    private RectTransform rectTransform;
    public Text name;
    private GameObject character;
    private TextMesh text;
    private GameObject[] equips;
    private GameObject[] shoes;
    private string[] strings = new string[] { "Shirt", "Pants", "Shoes", "Weapon" };
    public List<CreateCharacterButton> buttons = new List<CreateCharacterButton>();
    private GameObject[] skin;
    private GameObject[] eyes;
    private GameObject headTransform;
    public Login login;
	// Use this for initialization
	void Start () {
        this.rectTransform = this.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
    }
    public void setCharacter(GameObject character) {
        this.character = character;
        character.transform.GetChild(0).GetComponent<TextMesh>().text = "";
        this.text = this.character.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
        Debug.Log("child name " + this.character.name);
        equips = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Shirt", "Pants", "Weapon");
        skin = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "BodyModel", "HeadModel");
        shoes = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Shoe_L", "Shoe_R");
        eyes = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Eye_L_Model", "Eye_R_Model");
        this.headTransform = Tools.getChild(this.character.transform.GetChild(1).gameObject, "Head");
    }
    public void addButton(CreateCharacterButton btn) {
        buttons.Add(btn);
    }
	// Update is called once per frame
	void Update () {
        if (shouldSlide) {
            this.rectTransform.offsetMin = Vector2.Lerp(this.rectTransform.offsetMin, new Vector2(0, 0),3f * Time.deltaTime);
            this.rectTransform.offsetMax = Vector2.Lerp(this.rectTransform.offsetMax, new Vector2(0, 0), 3f * Time.deltaTime);
            if (this.rectTransform.offsetMin.x < 0.02f) {
                this.rectTransform.offsetMin = new Vector2(0, 0);
                this.rectTransform.offsetMax = new Vector2(0, 0);
                shouldSlide = false;
            }
        }
        if (loaded) {
            if (Input.anyKeyDown) {
                this.text.text = name.text;
            }
        }
	}
    private int getIndex(string type) {
        for (int i = 0; i < strings.Length; i++) {
            if (strings[i] == type) return i;
        }
        return -1;
    }
    public void equipItem(int itemID, string type) {
        if (type == "Shoe") {
            for (int i = 0; i < shoes.Length; i++)
            {
                Destroy(equips[i]);
                equips[i] = Instantiate(ResourceStructure.getGameObjectFromPath(ItemDataProvider.getInstance().getStats(itemID).getString("pathToModel")));
                equips[i].transform.SetParent(this.character.transform.GetChild(1));
                equips[i].GetComponent<SkinnedMeshRenderer>().rootBone = headTransform.transform;
            }
            return;
        }
        Debug.Log("EQUIPS LENGTH: " + type);
        int index = getIndex(type);
        Transform trans = equips[index].GetComponent<Transform>();
        GameObject tempItem = Instantiate(ResourceStructure.getGameObjectFromPath(ItemDataProvider.getInstance().getStats(itemID).getString("pathToModel")));
        equips[index].GetComponent<SkinnedMeshRenderer>().sharedMesh = tempItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        equips[index].GetComponent<SkinnedMeshRenderer>().sharedMaterial = tempItem.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        Destroy(tempItem);
    }
    public void changeColor(Color32 color, string type) {
        if (type == "Skin") {
            for (int i = 0; i < skin.Length; i++) {
                skin[i].GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color",color);
            }
        }
        else if (type == "Eye")
        {
            for (int i = 0; i < eyes.Length; i++)
            {
                eyes[i].GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", color);
            }
        }
    }
    public void onFinishCreating() {

    }
    public Text getName() {
        return name;
    }
    public Login getLogin()
    {
        return login;
    }
    public CreateCharacterButton[] getCheckedButtons() {
        List<CreateCharacterButton> btns = new List<CreateCharacterButton>();
        for (int i = 0; i < buttons.Count; i++) {
            if (buttons[i].isChecked) {
                Debug.Log("checked");
                btns.Add(buttons[i]);
            }
        }
        return btns.ToArray();
    }
    public List<CreateCharacterButton> getButtons() {
        return buttons;
    }
    public void slideIn() {
        this.shouldSlide = true;
        this.loaded = true;
    }
}
