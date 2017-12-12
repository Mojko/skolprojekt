using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillId : MonoBehaviour {
	public int id;
	public int points;
	public int pointsNeededFromParentToActivate;
	public int maxPoints;
	public Skill skill;
    public Text pointsText;
	public MouseOverUI mouse;
	public float damageMultiplier;
	[HideInInspector] public SkillTree skillTree;
    [HideInInspector] public Image image;
	public string pathToSkillModel;
	public SkillStats stats = new SkillStats();

	void Start(){
		this.mouse = this.GetComponent<MouseOverUI>();
        this.image = this.GetComponent<Image>();
	}
    private void init()
    {
        
    }
    //CmdSendSkillServerToServer(this.skill.pathToSkillModel, "Particles/Skills/Skill", pos, rot);
    /*            float[] pos = { player.transform.position.x, player.transform.position.y, player.transform.position.z };
            float[] rot = { player.getPlayerMovement().rot.x, player.getPlayerMovement().rot.y, player.getPlayerMovement().rot.z };
            CmdSendSkillServerToServer(this.skill.pathToSkillModel, "Particles/Skills/Skill", pos, rot);*/
    public void setSkillTree(SkillTree skillTree){
		this.skillTree = skillTree;
	}


    public bool isMouseHoveringOverUIElement(Vector3 pos)
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

        return (x1 < x2 + width && x1 + mouseWidth > x2 && y1 < y2 + mouseHeight && height + y1 > y2);
    }
	void Update(){
		
		if(isMouseHoveringOverUIElement(this.transform.position)){
			if(Input.GetMouseButtonDown(0)){
				skillTree.hasSkillBoxBeenClicked(this.gameObject, skill);
			}
			if(Input.GetMouseButtonDown(1)){
				skillTree.onSkillBoxSelect(this);
			}
		}
        if(pointsText != null){
            pointsText.text = points.ToString() + " / " + maxPoints.ToString();
        }
	}
}

public class SkillStats {
	public int STR = 0;
	public int INT = 0;
	public int DEX = 0;
}