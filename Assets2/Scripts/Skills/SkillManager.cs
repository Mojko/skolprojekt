using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour {

    public float cooldown;
    public Timer cooldownTimer;
    private bool cooldownStarted;

    private Skill skill;
    private GameObject skillPrefab;
    private Player player;


    public void init(Player player)
    {
        this.player = player;
        this.skillPrefab = player.skillPrefab;;
    }

    private void Update () 
    {
        if(this.cooldownTimer == null) return;
        if(cooldownStarted) this.cooldownTimer.update();

        if (this.cooldownTimer.isFinished()) {
            GameObject skillObject = Instantiate(skillPrefab);
            Debug.Log("PATH_TO_MODEL: " + skill.pathToSkillModel);
            GameObject skillEffect = Instantiate((GameObject)Resources.Load(skill.pathToSkillModel));
            skillEffect.transform.SetParent(skillObject.transform);
            skillObject.transform.position = new Vector3(this.player.transform.position.x, this.player.transform.position.y+1f, this.player.transform.position.z);
            skillObject.transform.rotation = this.player.GetComponent<PlayerMovement>().rot;
            stopCooldown();
            player.getPlayerMovement().animator.SetBool("magicAttack_1", false);
        }
	}

    public void castSkill(Skill skill)
    {
        if(cooldownStarted) return;
        this.skill = skill;
        startCooldown();
    }

    public void startCooldown()
    {
        cooldownStarted = true;
        this.cooldown = this.skill.cooldown;
        this.cooldownTimer = new Timer(cooldown, false);
        this.cooldownTimer.start();
    }
    public void stopCooldown()
    {
        cooldownStarted = false;
    }
}

/*
public enum e_BuffType
{
    HEAL,
    MANA,
    DAMAGE,
    ARMOR,
    CRIT
}
public enum e_CastType
{
    AOE,
    MULTI_SHOT
}

public class SkillCast : SkillManager
{
    e_CastType type;
    public SkillCast(e_CastType type)
    {
        this.type = type;
    }
    public override void onCast()
    {
        switch (type) {
            case e_CastType.AOE:
            break;
            case e_CastType.MULTI_SHOT:
            break;
        }
    }
}
public class SkillBuff : SkillManager
{
    e_BuffType type;
    public SkillBuff(e_BuffType type)
    {
        this.type = type;
    }
    public override void onCast()
    {
         switch (type) {
        }
    }
}
*/