﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum e_AnimationLayers {
	MAIN,
	MAGIC_SKILLS,
	WARRIOR_SKILLS,
	HUNTER_SKILLS
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

    [SyncVar] public NetworkInstanceId parentNetId;
    [SyncVar] public NetworkInstanceId childNetId;

	public void init(Player player, SkillUIManager skillUiManager)
    {
        this.player = player;
        this.skillPrefab = player.skillPrefab;
		this.skillUiManager = skillUiManager;
		this.animator = this.GetComponent<Animator>();
		hasInit = true;
    }

	private bool isAnimationFinished(string name){
		return !animator.GetCurrentAnimatorStateInfo((int)e_AnimationLayers.MAGIC_SKILLS).IsName(name) && cooldownStarted;
	}

    

    //BYT UT FLOAT[] ARRAY MOT VECTOR3 OCH QUATERNION
	[Command]
	void CmdSendSkillServerToServer(string pathToSkillEffect, float[] posInFloat, float[] rotInFloat){ //BYT UT FLOAT[] ARRAY MOT VECTOR3 OCH QUATERNION
        Vector3 pos = new Vector3(posInFloat[0], posInFloat[1], posInFloat[2]);
        Quaternion rot = Quaternion.Euler(new Vector3(rotInFloat[0], rotInFloat[1], rotInFloat[2]));

		GameObject skillEffect = Instantiate((GameObject)Resources.Load(pathToSkillEffect));
        GameObject activationEffect = Instantiate(skillEffect.GetComponent<SkillMovement>().activationEffect);

        skillEffect.transform.position = pos;
        activationEffect.transform.position = pos;
        skillEffect.transform.rotation = rot;
		
        NetworkServer.Spawn(skillEffect);
    }
    //BYT UT FLOAT[] ARRAY MOT VECTOR3 OCH QUATERNION
    private void Update () 
    {
		if(!hasInit || animator == null) return;

		if(isAnimationFinished(this.currentAnimationPlayingName)) {
            float[] pos = { player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z }; //BYT UT FLOAT[] ARRAY MOT VECTOR3 OCH QUATERNION
            float[] rot = { player.transform.rotation.eulerAngles.x, player.transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z }; //BYT UT FLOAT[] ARRAY MOT VECTOR3 OCH QUATERNION
            CmdSendSkillServerToServer(this.skill.pathToSkillModel, pos, rot);
            stopCooldown();
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