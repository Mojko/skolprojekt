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

    public string hairColor;
    public string skinColor;
    public string eyeColor;

    public int s_luk;
    public int s_str;
    public int s_int;
    public int s_dex;
}
