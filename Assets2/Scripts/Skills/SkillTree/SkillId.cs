using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillId : MonoBehaviour {
	public int id;
	public int points;
	public int pointsNeededFromParentToActivate;
	public int maxPoints;
    public string pathToSkillModel;
	public Skill skill;
	public MouseOverUI mouse;

	SkillTree skillTree;

	void Start(){
		this.mouse = this.GetComponent<MouseOverUI>();
		skillTree = this.transform.root.Find("SkillTree_UI").Find("SkillTree(Clone)").GetComponent<SkillTree>();
	}
	void Update(){
		if(mouse.isMouseOver() && Input.GetMouseButtonDown(0)){
			skillTree.hasSkillBoxBeenClicked(this.gameObject, skill);
		}
        if(mouse.isMouseOver() && Input.GetMouseButtonDown(1)) {
            skillTree.onSkillBoxSelect(this.skill);
        }
	}
}
