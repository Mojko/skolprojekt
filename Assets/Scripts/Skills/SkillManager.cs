using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum e_AnimationLayers {
	MAIN,
	MAGIC_SKILLS,
	WARRIOR_SKILLS,
	HUNTER_SKILLS
}
public enum e_SkillType {
	PROJECTILE,
	AOE,
	TARGET
}

public class SkillManager : NetworkBehaviour {

    private bool cooldownStarted;
	public SkillUIManager skillUiManager;

    private Skill skill;
    private GameObject skillPrefab;
    private Player player;
	private Animator animator;
	private string currentAnimationPlayingName;
	private bool hasInit;
	private Vector3 position;

	public void init(Player player, SkillUIManager skillUiManager)
    {
        this.player = player;
        this.skillPrefab = player.skillPrefab;
		this.skillUiManager = skillUiManager;
		this.animator = this.GetComponent<Animator>();
		hasInit = true;
    }

	public e_SkillType stringToSkillType(string name){
		if(name.Equals("projectile")) return e_SkillType.PROJECTILE;
		if(name.Equals("aoe")) return e_SkillType.AOE;
		if(name.Equals("target")) return e_SkillType.TARGET;
		return e_SkillType.AOE;
	}

	private bool isAnimationFinished(string name){
		return !animator.GetCurrentAnimatorStateInfo((int)e_AnimationLayers.MAGIC_SKILLS).IsName(name) && cooldownStarted;
	}

	public e_SkillType getSkillType(){
		return stringToSkillType(skill.type);
	}

    private void Update () 
    {
		if(!hasInit || animator == null) return;

		if(isAnimationFinished(this.currentAnimationPlayingName)) {
			Debug.Log("sending skillscast..");
			Vector3 offset = Tools.arrayToVector3(skill.positionOffset);

			Vector3 relativePosition = transform.TransformDirection(offset) * offset.magnitude;

			player.getNetwork().sendSkillCast(this.skill.pathToSkillModel, relativePosition, player.getPlayerMovement().rot.eulerAngles, this.skill.type, this.skill.range, (e_DamageType)this.skill.damageType, this.skill.damageMultiplier);

            stopCooldown();
			player.getPlayerMovement().unfreeze();
        }
	}

	public Skill isPlayerTryingToActivateSkill()
	{
		Dictionary<int, KeyCode> keys = this.skillUiManager.getKeys();
		ActionBar[] actionBars = this.skillUiManager.getActionBars();

		if(actionBars.Length > keys.Count){
			for(int i=0;i<keys.Count;i++){
				if (Input.GetKey(keys[i]) && actionBars[i].skill.id != 0) {
					return actionBars[i].skill;
				}
			}
		} else {
			Debug.LogError("More bound keys than actionbars! (SkillManager.cs)");
		}
		return null;
	}

	public void castSkill(Skill skill, string animationName)
    {
        if(cooldownStarted) return;

		this.player.getPlayerMovement().animator.Play(skill.name);
        this.skill = skill;
		this.currentAnimationPlayingName = animationName;
        startCooldown();

		if(!skill.pathToPreEffect.Equals(string.Empty)){
			Instantiate((GameObject)Resources.Load(skill.pathToPreEffect)).transform.position = new Vector3(this.transform.position.x, this.transform.position.y+1, this.transform.position.z);//this.transform.position;
		}
    }

	public bool isCasting(){
		return this.cooldownStarted;
	}

    public void startCooldown()
    {
        cooldownStarted = true;
    }
    public void stopCooldown()
    {
        cooldownStarted = false;
    }
}