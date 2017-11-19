using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerHandler : MonoBehaviour {
    public Text nameUI;
    public Text levelUI;
    public RectTransform healthBar;
    public RectTransform manaBar;
    public Text[] healthBarText, manaBarText;
    private Player player;
    // Use this for initialization
    void Start () {
		
	}
    public void setPlayer(Player player) {
        this.player = player;
        this.nameUI.text = player.playerName;
        this.levelUI.text = "Level " + player.level;
        onHealthChange();
        onManaChange();
    }
	// Update is called once per frame
	void Update () {
		
	}
    public void onHealthChange() {
        Debug.Log("irhgt msadffdsaasfdafsdsdfaafsdasfdafsdfdsaadfsfadsafdsasdfdfsadafsasdffdsaadsfdsafdsfa8: " + 150 * (((float)player.maxHealth - (float)player.health) / (float)player.maxHealth));
        healthBar.offsetMax = new Vector2(-150 * (((float)player.maxHealth - (float)player.health) / (float)player.maxHealth), 0);
        healthBarText[0].text = player.health + "/" + player.maxHealth;
        healthBarText[1].text = player.health + "/" + player.maxHealth;
    }
    public void onManaChange()
    {
        manaBar.offsetMax = new Vector2(-150 * (((float)player.maxMana - (float)player.mana) / (float)player.maxMana), 0);
        manaBarText[0].text = player.mana + "/" + player.maxMana;
        manaBarText[0].text = player.mana + "/" + player.maxMana;
    }
}
