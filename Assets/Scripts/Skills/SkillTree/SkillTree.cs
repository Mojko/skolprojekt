using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using UnityEngine.UI;

public class SkillTree : UIHandler {

	public GameObject skillTreeSlotPrefab;
	public playerNetwork playerNetwork;
	public Player player;
	public SkillUIManager skillUi;
	public GameObject linePrefab;
	public Sprite[] spritesheet;
	public Sprite[] spritesheet_transparent;

    public GameObject pointsTextPrefab;

	bool confirmed = true;
	GameObject objectThatGotSent;
	GameObject potrait;
    public Material grayScaledMaterial;

	int xType = -1;
	int yType = -1;

	float xRoot = Screen.width/2;
	float yRoot = Screen.height/2;
	float zRoot = 0;
	int currentIndex = 0;

    public float masterSpellWidthOffset; // 50
    public float masterSpellHeightOffset; // 90

    public float otherSpellWidthOffset; //25
    public float otherSpellHeightOffset; //100


	[HideInInspector]
	public GameObject UISkillStats;
	private GameObject UISkillStatsWrapper;
	private Text UISkillStatsName;
	private Text UISkillStatsPoints;
	[HideInInspector]
	public List<SkillId> skillsObjects = new List<SkillId>();
	[HideInInspector]
	public SkillId UISkillStatsWatcher;
	private RectTransform UISkillStatsRectTransform;

	public void initilize (Player player) {
		

		playerNetwork = player.getNetwork ();
		this.player = player;

		GameObject UI = GameObject.Find("UI");
        GameObject skillTreeUI = Tools.findInactiveChild(UI, "SkillTree_UI");
        GameObject panel = Tools.findInactiveChild(skillTreeUI, "Panel");
        GameObject skillTreeContainer = Tools.findInactiveChild(UI, "SkillTreeContainer");

		this.spritesheet = Resources.LoadAll<Sprite>("Sprites/SkillIcons/spritesheet");
		this.spritesheet_transparent = Resources.LoadAll<Sprite>("Sprites/SkillIcons/spritesheet_transparent");

		transform.SetParent(skillTreeUI.transform);
		this.UISkillStats = Tools.findInactiveChild(this.transform.parent.gameObject, "Panel").transform.Find("Stats_BG").gameObject;

		this.UISkillStatsWrapper = Tools.findInactiveChild(UISkillStats, "Wrapper");
		this.UISkillStatsPoints = Tools.findInactiveChild(UISkillStats, "Points").GetComponent<Text>();
		this.UISkillStatsName = Tools.findInactiveChild(UISkillStats, "Name").GetComponent<Text>();

		UISkillStatsRectTransform = this.UISkillStats.GetComponent<RectTransform>();

		//transform.SetParent(UI.transform.Find("SkillTree_UI").Find("Panel").Find("SkillTreeContainer"));
        Debug.Log("INTIILIZEDIDDDDDDD!!!!");
        UI.GetComponent<UIReferences>().skillTreeReference = this.gameObject;

		Skill skills = JsonManager.readJson<Skill>(e_Paths.JSON_SKILLTREE);

		Debug.Log("e_paths.jsonskilltree: " + JsonManager.getPath(e_Paths.JSON_SKILLTREE));
		Debug.Log("skills: " + skills);

		potrait = Instantiate(skillTreeSlotPrefab);
		Debug.Log("Is  this running? " + potrait);
		potrait.GetComponent<SkillId>().setSkillTree(this);
		potrait.transform.SetParent(this.transform);
		potrait.transform.position = new Vector3(xRoot, yRoot, zRoot);
        potrait.transform.localScale += new Vector3(1,1,0);
		potrait.GetComponent<RectTransform>().sizeDelta = new Vector3(30,35,0);
		potrait.GetComponent<Image>().sprite = spritesheet[0];
		onSkillUpgrade(potrait, potrait.GetComponent<SkillId>(), this.gameObject, null);

		foreach(Skill skill in skills.Potrait){
			chooseType(currentIndex);
			currentIndex++;
			//Create parents
			GameObject inst = Instantiate(skillTreeSlotPrefab);
			SkillId skillId = inst.GetComponent<SkillId>();
			skillId.setSkillTree(this);
			skillsObjects.Add(skillId);
			inst.transform.SetParent(potrait.transform);
			createSkillObject(inst, skill);

            masterSpellWidthOffset = 35*potrait.transform.localScale.x;
            masterSpellHeightOffset = 35*potrait.transform.localScale.y;

			inst.transform.position = new Vector3(xRoot+(masterSpellWidthOffset*xType), yRoot+(masterSpellHeightOffset*yType), zRoot);
            createText(inst, false);

			onSkillUpgrade(inst, skillId, potrait, skill);

			if(hasSkillChildren(skill)){
				parseChildren(skill.children, skill, inst);
			}
		}
	}

	void chooseType(int i){
		if(i == 1) xType = -xType;
		if(i == 2) yType = -yType;
		if(i == 3) xType = -xType;
	}
    void createText(GameObject inst, bool isEnhancement)
    {

        float scale = 0;
        float positionX = 0;
        float positionY = 0;

        if(isEnhancement) {
            positionX = 0;
            positionY = 15;
            scale = 2.1f;
        } else {
            positionX = 0;
            positionY = 22;
            scale = 1.6f;
        }

        GameObject o = Instantiate(pointsTextPrefab);
        o.transform.localScale = new Vector3(o.transform.lossyScale.x/scale, o.transform.lossyScale.y/scale, o.transform.lossyScale.z);
        o.GetComponent<RectTransform>().anchoredPosition = new Vector3(inst.transform.position.x - positionX, inst.transform.position.y - positionY, inst.transform.position.z);
        o.transform.SetParent(potrait.transform);
        if(isSkill(inst)){
            inst.GetComponent<SkillId>().pointsText = o.GetComponent<Text>();
        }
    }

	public void hoveringOverSkill(SkillId skillId){
		UISkillStats.SetActive(true);
		this.UISkillStats.transform.position = new Vector3(skillId.transform.position.x, skillId.transform.position.y+1f, skillId.transform.position.z);
	}

	void parseChildren(Skill[] children, Skill parent, GameObject parentObject){
		foreach(Skill skill in children){
			if(parent != null){
				skill.parent = parent;
			}
			GameObject inst = Instantiate(skillTreeSlotPrefab);
			SkillId skillId = inst.GetComponent<SkillId>();
			skillId.setSkillTree(this);
			skillsObjects.Add(skillId);
			createSkillObject(inst, skill);


			if(parentObject != null){
				inst.transform.SetParent(parentObject.transform);
				onSkillUpgrade(inst, skillId, parentObject, skill);
			}


			int x1 = 0;
			int y1 = 1;
            float scale = 1;
            float positionY = 20f;

			if(skill.enhancementSkill){
				y1 = 0;
				x1 = 1;
				inst.transform.localScale = new Vector3(inst.transform.localScale.x/1.5f, inst.transform.localScale.y/1.5f, 1);
                scale = 2f;
                positionY = 15;
			}
			if(parentObject != null){
				inst.transform.position = new Vector3(parentObject.transform.position.x+(((otherSpellWidthOffset)+parentObject.GetComponent<RectTransform>().sizeDelta.x)*x1)*xType, parentObject.transform.position.y+((otherSpellHeightOffset*yType)*y1), zRoot);
                createText(inst, skill.enhancementSkill);
			}

			if(hasSkillChildren(skill)){
				parseChildren(skill.children, skill, inst);
			}
		}
	}

	public bool isEnabled() {
		return isActive;
	}

	void Update(){
        /*
		if(Input.GetKey(KeyCode.K)) potrait.transform.position += new Vector3(0, -5, 0);
		if(Input.GetKey(KeyCode.O)) potrait.transform.position += new Vector3(0, 5, 0);
		if(Input.GetKey(KeyCode.L)) potrait.transform.position += new Vector3(5, 0, 0);
		if(Input.GetKey(KeyCode.J)) potrait.transform.position += new Vector3(-5, 0, 0);*/
		if(potrait != null){
	        if(Input.GetAxis("Mouse ScrollWheel") < 0 && potrait.transform.localScale.x > 0.5f) {
	            potrait.transform.localScale -= new Vector3(0.1f, 0.1f, 0);
	        }
	        if(Input.GetAxis("Mouse ScrollWheel") > 0) {
	            potrait.transform.localScale += new Vector3(0.1f, 0.1f, 0);
	        }
		}

		foreach(SkillId s in this.skillsObjects){
			if(s.isMouseHoveringOverUIElement(s.transform.position)){
				this.UISkillStats.SetActive(true);
				this.UISkillStats.transform.position = new Vector3(s.transform.position.x, s.transform.position.y+120f, s.transform.position.z);
				this.UISkillStatsWatcher = s;
				updateSkillStatsUI(s);
				return;
			}
		}
		if(this.UISkillStatsWatcher != null){
			if(!this.UISkillStatsWatcher.isMouseHoveringOverUIElement(this.UISkillStatsWatcher.transform.position)){
				this.UISkillStats.SetActive(false);
				this.UISkillStatsWatcher = null;
			}
		}

		//this.UISkillStats.transform.position = new Vector2(Input.mousePosition.x + UISkillStatsRectTransform.sizeDelta.x/2, Input.mousePosition.y + UISkillStatsRectTransform.sizeDelta.y/2);
	}

	public void updateSkillStatsUI(SkillId s){
		Text[] t = UISkillStatsWrapper.GetComponentsInChildren<Text>();
		t[0].text = "Strength: +" + s.stats.STR;
		t[1].text = "Dexterity: +" + s.stats.DEX;
		t[2].text = "Intelligence: +" + s.stats.INT;
		UISkillStatsPoints.text = s.points + " / " + s.maxPoints;
		UISkillStatsName.text = s.skill.name;
		UISkillStatsName.fontSize = (int)Mathf.Round(100/UISkillStatsName.text.Length);
	}

	public void confirmSkills(){
		SkillId skillId = objectThatGotSent.GetComponent<SkillId>();
		skillId.points += 1;
		skillId.damageMultiplier *= (skillId.points * 0.3f);
		this.player.stats.increment(skillId.stats.STR, skillId.stats.DEX, skillId.stats.INT, 0,0,0,0,0,0,0,this.player.uiStats);
		onSkillUpgrade(objectThatGotSent, skillId, objectThatGotSent.transform.parent.gameObject, skillId.skill);
		objectThatGotSent = null;
		this.updateSkillStatsUI(skillId);
	}

	public void hasSkillBoxBeenClicked(GameObject obj, Skill skill){
		if(!isSkill(skill)) return;
		if(isSkillUpgradeable(skill, obj.transform.parent.gameObject)){
			objectThatGotSent = obj;
			playerNetwork.sendSkill(obj);
		}

	}
	public void onSkillBoxSelect(SkillId skillId)
	{
        Skill skill = skillId.skill;
		if(!isSkill(skill) || skillId.points < 1) return;
		player.getSkillUiManager().grabSkill(skill);
		this.transform.parent.gameObject.SetActive(false);
	}

	public void uiToggleVisibility(GameObject obj, bool val){
		obj.SetActive(val);
	}

	bool isSkill(GameObject obj){
		SkillId skillId = obj.GetComponent<SkillId>();
		if(obj.CompareTag("Skill") && skillId.id > 0){
			return true;
		}
		return false;
	}
	bool isSkill(Skill skill){
		if(skill.id > 0){
			return true;
		}
		return false;
	}

    bool isSkillActivated(Image image)
    {
        if(image.material != null) {
            return true;
        } else {
            return false;
        }
    }

	void onSkillUpgrade(GameObject obj, SkillId skillId, GameObject parent, Skill skill){
		Image objImage = obj.GetComponent<Image>();
		Image parentImage = null;
        //Color notActivated = new Color(255,255,255,0.3f);
		//Color activated = new Color(255,255,255,1);
        

		if(isSkill(parent)){
			parentImage = parent.GetComponent<Image>();

            if (!isSkillActivated(parentImage)) {
                objImage.material = this.grayScaledMaterial;
                return;
            }

            if (isSkillUpgradeable(skill, parent)) {
                objImage.material = null;
            } else {
                objImage.material = this.grayScaledMaterial;
            }
		} else {
			objImage.material = null;
		}

		/*if(parentImage.color != activated) { objImage.color = notActivated; return; }
		if(isSkillUpgradeable(skill, parent)){
			objImage.color = activated;
		} else {
			objImage.color = notActivated;
		}*/

		for(int i=0;i<obj.transform.childCount;i++){
			SkillId s = obj.transform.GetChild(i).GetComponent<SkillId>();

			if(isSkillUpgradeable(s.skill, obj)){
				onSkillUpgrade(obj.transform.GetChild(i).gameObject,
					s,
					obj,
					s.skill);
			} else {
				break;
			}
		}
	}

	bool isSkillUpgradeable(Skill skill, GameObject parent){
		if(parent.CompareTag("Skill")){
			if(skill.pointsNeededFromParentToActivate <= parent.GetComponent<SkillId>().points){
				return true;
			}
		}
		return false;
	}

	bool isSkillUpgradeable(Skill skill, int parentPoints){
		if(skill.pointsNeededFromParentToActivate <= parentPoints){
			return true;
		}
		return false;
	}
	bool hasSkillChildren(Skill skill){
		if(skill.children != null){
			if(skill.children.Length > 0){
				return true;
			}
		}
		return false;
	}

	GameObject createSkillTreeSlot(GameObject prefab){
		return Instantiate(skillTreeSlotPrefab);
	}

	void createSkillObject(GameObject inst, Skill skill){
		SkillId skillId = inst.GetComponent<SkillId>();
		skillId.id = skill.id;

		foreach(Skill s in this.player.skillsToVerifyWithFromServer){
			if(s.id == skill.id){
				skillId.points = s.currentPoints;
			}
		}

		skillId.pointsNeededFromParentToActivate = skill.pointsNeededFromParentToActivate;
		skillId.skill = skill;
		skillId.maxPoints = skill.maxPoints;
		skillId.pathToSkillModel = skill.pathToSkillModel;

        skillId.image = inst.GetComponent<Image>();
		int index = (skillId.id-DefaultIds.skillDefaultId)+1;
		Debug.Log("SPRITE INDEX: " + index + " | " + skillId.id + " | " + DefaultIds.skillDefaultId + " | " + (skillId.id-DefaultIds.skillDefaultId) + " | " + ((skillId.id-DefaultIds.skillDefaultId)+1));
		skillId.image.sprite = this.spritesheet[index];

		skillId.stats.STR = skill.str;
		skillId.stats.INT = skill.intell;
		skillId.stats.DEX = skill.dex;
		skill.damageMultiplier *= (skillId.points * 0.3f);
		skillId.damageMultiplier = skill.damageMultiplier;
		index = (skillId.id-DefaultIds.skillDefaultId);
		skill.sprite = this.spritesheet_transparent[index];
		this.player.skills.Add(skill);
		
	}

	GameObject getParent(GameObject child){
		return child.transform.parent.gameObject;
	}}

[System.Serializable]
public class Skill {
	public Skill(int id, int currentPoints, int maxPoints){
		this.id = id;
		this.currentPoints = currentPoints;
		this.maxPoints = maxPoints;
	}	
	public Skill(){

	}

	public Skill[] Magician;
	public int id;
	public string name;
	public int pointsNeededFromParentToActivate;
	public int currentPoints;
	public int maxPoints;
	public int[] activateIds;
	public string playerName;
	public Skill[] children;
	public Skill parent;
	public Skill[] Potrait;
	public bool enhancementSkill;
	public string pathToSkillModel;
	public string pathToPreEffect;
	public int cooldown;
	public string type;
	public Sprite sprite;
	public int[] positionOffset;
	public int range;
	public int str;
	public int dex;
	public int intell;
	public int damageType;
	public float damageMultiplier;
}
