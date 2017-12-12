using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class PlayerStats {
    public int level = 0;
    public int exp = 0;
    public int health = 0;
    public int mana = 0;
    public int maxHealth = 100;
    public int maxMana = 100;
	public int money = 0;
	public int expRequiredForNextLevel = 999999999;

    public string hairColor;
    public string skinColor;
    public string eyeColor;

    public int s_luk;
    public int s_str;
    public int s_int;
    public int s_dex;

	public int maxAttackDamage;
	public int minAttackDamage;
	public int maxMagicDamage;
	public int minMagicDamage;


	/*

	s_watt s_matt ska förändras beroende på vad man har på sig.

	*/

	public int s_watt;
	public int s_matt;

	public void increment(int str, int dex, int s_int, int watt, int matt, int health, int mana, int exp, int level, int money, UIStats uistats){
		this.s_str += str;
		this.s_dex += dex;
		this.s_int += s_int;
		this.s_watt += watt;
		this.s_matt += matt;
		this.health += health;
		this.mana += mana;
		this.exp += exp;
		this.level += level;
		this.money += money;
		uistats.update(this);
	}
}
