using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum e_Stats {
	STR,
	DEX,
	INT,
	WATT,
	MATT
}

public class UIStats : MonoBehaviour {

	public Text[] stats;

	public void update(PlayerStats stats){
		this.stats[(int)e_Stats.STR].text = "Strength: " + stats.s_str.ToString();
		this.stats[(int)e_Stats.DEX].text = "Dexterity: " + stats.s_dex.ToString();
		this.stats[(int)e_Stats.INT].text = "Intelligence: " + stats.s_int.ToString();
		this.stats[(int)e_Stats.WATT].text = "Weapon Attack: " + stats.s_watt.ToString();
		this.stats[(int)e_Stats.MATT].text = "Magic Attack: " + stats.s_matt.ToString();
	}
}
