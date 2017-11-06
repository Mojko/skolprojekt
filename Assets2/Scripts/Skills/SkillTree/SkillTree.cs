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

	bool confirmed = true;
	GameObject objectThatGotSent;

	GameObject potrait;

	int xType = -1;
	int yType = -1;

	string path;
	string jsonString;

	float xRoot = 300;
	float yRoot = 290;
	float zRoot = 0;
	int currentIndex = 0;

	public void initilize (Player player) {
		playerNetwork = player.getNetwork ();
        this.player = player;
		path = "Assets/XML/SkillTree.json";
		jsonString = File.ReadAllText (path);

        transform.SetParent(GameObject.Find("UI").transform.Find("SkillTree_UI"));

		Skill skills = JsonUtility.FromJson<Skill> (jsonString);

		potrait = Instantiate(skillTreeSlotPrefab);
		potrait.transform.SetParent(this.transform);
		potrait.transform.position = new Vector3(xRoot, yRoot, zRoot);
		onSkillUpgrade(potrait, potrait.GetComponent<SkillId>(), this.gameObject, null);

		foreach(Skill skill in skills.Potrait){
			chooseType(currentIndex);
			currentIndex++;
			//Create parents
			GameObject inst = Instantiate(skillTreeSlotPrefab);
			inst.transform.SetParent(potrait.transform);
			createSkillObject(inst, skill);

			inst.transform.position = new Vector3(xRoot+(50*xType), yRoot+(75*yType), zRoot);

			onSkillUpgrade(inst, inst.GetComponent<SkillId>(), potrait, skill);

			if(hasSkillChildren(skill)){
				parseChildren(skill.children, skill);
			}
		}
    }

    void chooseType(int i){
    	if(i == 1) xType = -xType;
    	if(i == 2) yType = -yType;
    	if(i == 3) xType = -xType;
    }

	void parseChildren(Skill[] children, Skill parent){
    	foreach(Skill skill in children){
    		if(parent != null){
    			skill.parent = parent;
    		}
			GameObject inst = Instantiate(skillTreeSlotPrefab);
			createSkillObject(inst, skill);
			GameObject[] o = GameObject.FindGameObjectsWithTag("Skill");
			GameObject p = null;
			for(int i=0;i<o.Length;i++){
				if(o[i].GetComponent<SkillId>().id == parent.id){
					p = o[i];
					break;
				}
			}
            if(p != null){
			    inst.transform.SetParent(p.transform);
                onSkillUpgrade(inst, inst.GetComponent<SkillId>(), p, skill);
            }
			

			int x1 = 0;
			int y1 = 1;

			if(skill.enhancementSkill){
				y1 = 0;
				x1 = 1;
				inst.transform.localScale = new Vector3(inst.transform.localScale.x/1.5f, inst.transform.localScale.y/1.5f, 1);
			}
            if(p != null){
			    inst.transform.position = new Vector3(p.transform.position.x+(((25)+p.GetComponent<RectTransform>().sizeDelta.x)*x1)*xType, p.transform.position.y+((100*yType)*y1), zRoot);
            }

			if(hasSkillChildren(skill)){
				parseChildren(skill.children, skill);
			}
		}
    }

	public bool isEnabled() {
        return isActive;
    }

    void Update(){
    	if(Input.GetKey(KeyCode.K)) potrait.transform.position += new Vector3(0, -5, 0);
		if(Input.GetKey(KeyCode.O)) potrait.transform.position += new Vector3(0, 5, 0);
		if(Input.GetKey(KeyCode.L)) potrait.transform.position += new Vector3(5, 0, 0);
		if(Input.GetKey(KeyCode.J)) potrait.transform.position += new Vector3(-5, 0, 0);
    }

    public void confirmSkills(){
		objectThatGotSent.GetComponent<SkillId>().points += 1;
		onSkillUpgrade(objectThatGotSent, objectThatGotSent.GetComponent<SkillId>(), objectThatGotSent.transform.parent.gameObject, objectThatGotSent.GetComponent<SkillId>().skill);
		objectThatGotSent = null;
    }

	public void hasSkillBoxBeenClicked(GameObject obj, Skill skill){
		if(isSkillUpgradeable(skill, obj.transform.parent.gameObject)){
			objectThatGotSent = obj;
			playerNetwork.sendSkill(obj);
		}
	}
    public void onSkillBoxSelect(Skill skill)
    {
        player.getSkillUiManager().addNewSkillToActionBar(skill);
	}

	public void uiToggleVisibility(GameObject obj, bool val){
		obj.SetActive(val);
	}

    bool isSkill(GameObject obj){
    	if(obj.CompareTag("Skill")){
    		return true;
    	}
    	return false;
    }

    void onSkillUpgrade(GameObject obj, SkillId skillId, GameObject parent, Skill skill){
    	Image objImage = obj.GetComponent<Image>();
    	Image parentImage = null;
    	Color notActivated = new Color(255,255,255,0.3f);
    	Color activated = new Color(255,255,255,1);

    	if(isSkill(parent)){
			parentImage = parent.GetComponent<Image>();

			if(parentImage.color != activated) { objImage.color = notActivated; return; }
			if(isSkillUpgradeable(skill, parent)){
				objImage.color = activated;
	    	} else {
	    		objImage.color = notActivated;
	    	}

    	} else {
    		objImage.color = activated;
    	} 

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
        this.player.skills.Add(skill);
    }

    GameObject getParent(GameObject child){
    	return child.transform.parent.gameObject;
    }
}

[System.Serializable]
public class Skill {
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
    public int cooldown;
    public string type;
}

    /*void parseSkill(Skill skill){
		GameObject inst = Instantiate(skillTreeSlotPrefab);
		inst = setParents(inst, this.gameObject, cache);
		inst.transform.position = new Vector3(xRoot,yRoot,zRoot);
		inst.transform.localScale = new Vector2(inst.transform.localScale.x/1.5f, inst.transform.localScale.y/1.5f);
		tempObjects.Add(inst);
		childCount--;
		createSkillObject(inst, skill);

		Debug.Log("cCount: " + childCount);

		if(childCount <= 0){
			for(int i=0;i<tempSkills.Count;i++){
				cache = inst;
				childCount = skill.children.Length;
				Debug.Log("NAME: " + tempSkills[i].name);
				if(hasSkillChildren(tempSkills[i])){
					parseChildren(tempSkills[i].children);
				}
			}
		}
    }*/