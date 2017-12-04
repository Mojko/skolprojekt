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
    private string[] stringsColor = new string[] { "HairColor", "SkinColor", "EyeColor"};

    private GameObject[] skin;
    private GameObject headTransform;
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
        Debug.Log("equips length: " + equips.Length);
        shoes = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Shoe_L", "Shoe_R");
        this.headTransform = Tools.getChild(this.character.transform.GetChild(1).gameObject, "Head");
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
    private int getIndex(string type, string[] array) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i] == type) return i;
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
        int index = getIndex(type, strings);
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
    }
    public void slideIn() {
        this.shouldSlide = true;
        this.loaded = true;
    }
}
