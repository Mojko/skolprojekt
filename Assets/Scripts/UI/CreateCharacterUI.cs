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
    private string[] strings = new string[] { "Shirt", "Pants", "Shoes", "Wapons" };
    private GameObject headTransform;
	// Use this for initialization
	void Start () {
        this.rectTransform = this.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        this.headTransform = Tools.getChild(this.gameObject, "Head");
        Debug.Log("child name " + this.character.transform.GetChild(1).gameObject.name);
        equips = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Shirt","Pants","Weapon");
        shoes = Tools.getChildren(this.character.transform.GetChild(1).gameObject, "Shoe_L", "Shoe_R");
    }
    public void setCharacter(GameObject character) {
        this.character = character;
        character.transform.GetChild(0).GetComponent<TextMesh>().text = "";
        this.text = this.character.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
    }
	// Update is called once per frame
	void Update () {
        if (shouldSlide) {
            this.rectTransform.offsetMin = Vector2.Lerp(this.rectTransform.offsetMin, new Vector2(0, 0),3f* Time.deltaTime);
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
                equips[i] = Instantiate(ResourceStructure.getGameObjectFromPath(ItemDataProvider.getInstance().getStats(itemID).getString("pahtToModel")));
                equips[i].transform.SetParent(this.character.transform);
                equips[i].GetComponent<SkinnedMeshRenderer>().rootBone = headTransform.transform;
            }
            return;
        }
        int index = getIndex(type);
        Destroy(equips[index]);
        equips[index] = Instantiate(ResourceStructure.getGameObjectFromPath(ItemDataProvider.getInstance().getStats(itemID).getString("pahtToModel")));
        equips[index].transform.SetParent(this.character.transform);
        equips[index].GetComponent<SkinnedMeshRenderer>().rootBone = headTransform.transform;
    }
    public void slideIn() {
        this.shouldSlide = true;
        this.loaded = true;
    }
}
