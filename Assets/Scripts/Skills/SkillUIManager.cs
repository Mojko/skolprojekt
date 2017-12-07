using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour {
    private float xRoot = -75;
    private float yRoot = -15;
    private Player player;
    public GameObject skillIconPrefab;
	public UIHolderImage uiHolderImage;
	public ActionBar[] actionBars;

	private Skill skillHolding;
	private List<Skill> skillsInActionBar = new List<Skill>();
    private Dictionary<int, KeyCode> keys = new Dictionary<int, KeyCode>();

    private float maxSpaceInAbilityBar = 4;
    

    public void init(Player player)
    {
        this.player = player;
		bindKey(KeyCode.F1, 0);
		bindKey(KeyCode.F2, 1);
		bindKey(KeyCode.F3, 2);
    }
	void Update(){
		foreach(ActionBar a in this.actionBars){
			if(isMouseHoveringOverUIElement(a.transform.position)){
				if(Input.GetMouseButton(0) && this.skillHolding != null){
					addNewSkillToActionBar(this.skillHolding, a);
					return;
				} else if(Input.GetMouseButton(1) && this.skillHolding == null && a.skill != null){
					Skill s = null;
					removeSkillFromActionBar(a, out s);
					grabSkill(s);
				}
			}

		}
	}

	public void grabSkill(Skill skill){
		if(skill == null) return;
		this.skillHolding = skill;
		uiHolderImage.followMouse(skill.sprite);
	}

	public void dropSkill(){
		uiHolderImage.stopFollowMouse();
		skillHolding = null;
	}

	public void addNewSkillToActionBar(Skill skill, ActionBar actionBar)
    {
		if(skillsInActionBar.Count >= maxSpaceInAbilityBar || skill == null) return;
		dropSkill();
		Skill skillInActionBar = actionBar.skill;


		if(skillInActionBar == null || skillInActionBar.id != 0){
			grabSkill(actionBar.skill);
		}

		GameObject skillIcon = Instantiate(this.skillIconPrefab);
		skillsInActionBar.Add(skill);
		actionBar.skill = skill;
		actionBar.skillIcon = skillIcon;
		skillIcon.transform.SetParent(actionBar.transform);
		skillIcon.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		skillIcon.GetComponent<Image>().sprite = skill.sprite;

		Debug.Log("added skill to skillbar");
    }

	/*public void replaceSkillInActionBar(ActionBar actionBar, Skill newSkill){
		actionBar.skill = newSkill;
		Destroy(actionBar.skillIcon);
		addNewSkillToActionBar(newSkill, actionBar);
	}*/

	public void removeSkillFromActionBar(ActionBar actionBar, out Skill skill){
		skill = actionBar.skill;
		actionBar.skill = null;
		skillsInActionBar.Remove(skill);
		Destroy(actionBar.skillIcon);
	}


	//Put this in Tools
	bool isMouseHoveringOverUIElement(Vector3 pos)
	{
		float x1 = pos.x;
		float y1 = pos.y;
		float x2 = Input.mousePosition.x;
		float y2 = Input.mousePosition.y;

		RectTransform rect = this.GetComponent<RectTransform>();

		float mouseWidth = rect.sizeDelta.x;
		float mouseHeight = rect.sizeDelta.y;
		float scale = rect.localScale.x;
		float width = rect.sizeDelta.x;
		float height = rect.sizeDelta.y;

		return (x1 < x2 + (width/1.25f) && x1 + mouseWidth > x2 && y1 < y2 + mouseHeight && (height/1.25f) + y1 > y2);
	}

    public void bindKey(KeyCode key, int skillBox)
    {
        keys.Add(skillBox, key);
    }

	public Dictionary<int, KeyCode> getKeys(){
		return this.keys;
	}
	public ActionBar[] getActionBars(){
		return this.actionBars;
	}
}
