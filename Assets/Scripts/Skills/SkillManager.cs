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
			player.getNetwork().sendSkillCast(this.skill.pathToSkillModel, player.transform.position, player.getPlayerMovement().rot.eulerAngles, this.skill.type);

            stopCooldown();
			player.getPlayerMovement().unfreeze();
        }
	}

	public Skill isPlayerTryingToActivateSkill()
	{
		Dictionary<int, KeyCode> keys = this.skillUiManager.getKeys();
		Skill[] skillsInActionBar = this.skillUiManager.getSkillsInActionBar();

		if(skillsInActionBar.Length > 0){
			for(int i=0;i<keys.Count;i++){
				if (Input.GetKey(keys[i])) {
					return skillsInActionBar[i];
				}
			}
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