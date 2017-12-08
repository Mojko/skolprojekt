using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerHandler : MonoBehaviour {
    public Text levelUI;
    public RectTransform healthBar;
    public RectTransform manaBar;
    public RectTransform expBar;
    public Text[] healthBarText, manaBarText, expBarText;
    private Player player;
    // Use this for initialization
    void Start () {
		
	}
    public void setPlayer(Player player) {
        this.player = player;
        onHealthChange();
        onManaChange();
        onExpChange();
        onLevelChange();
    }
	// Update is called once per frame
	void Update () {
		
	}
    public void updateInfo() {
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! loaded");
        onHealthChange();
        onManaChange();
        onExpChange();
        onLevelChange();
    }
    public void onExpChange()
    {
        expBar.offsetMax = new Vector2(-150 * (((float)player.stats.expRequiredForNextLevel - (float)player.stats.exp) / (float)player.stats.expRequiredForNextLevel), 0);
        this.expBarText[0].text = player.stats.exp + " / " + player.stats.expRequiredForNextLevel;
        this.expBarText[1].text = player.stats.exp + " / " + player.stats.expRequiredForNextLevel;
    }
    public void onLevelChange()
    {
        this.levelUI.text = "Level " + player.stats.level;
    }
    public void onHealthChange() {
        healthBar.offsetMax = new Vector2(-150 * (((float)player.stats.maxHealth - (float)player.stats.health) / (float)player.stats.maxHealth), 0);
        healthBarText[0].text = player.stats.health + "/" + player.stats.maxHealth;
        healthBarText[1].text = player.stats.health + "/" + player.stats.maxHealth;
    }
    public void onManaChange()
    {
        manaBar.offsetMax = new Vector2(-150 * (((float)player.stats.maxMana - (float)player.stats.mana) / (float)player.stats.maxMana), 0);
        manaBarText[0].text = player.stats.mana + "/" + player.stats.maxMana;
        manaBarText[0].text = player.stats.mana + "/" + player.stats.maxMana;
    }
}
