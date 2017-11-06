using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Datum: 2017-09-22
	Funktion: Här ska alla player stats ligga.
*/

public class PlayerStats : MonoBehaviour {
	
	int health = 100;
	int armor = 0;

	public void damage (int amount) {
		health -= amount;
		Debug.Log("You got damaged! " + amount);
	}
	public void heal (int amount) {
		health += amount;
	}
}
