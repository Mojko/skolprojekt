using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
public class CreateCharacterSend : MonoBehaviour {
    public CreateCharacterUI characterUI;
    public Login login;
    public Text errorText;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onClick() {
        this.errorText.text = "";
        string name = characterUI.getName().text;
        if (name.Length >= 15 || name.Length < 2) {
            this.errorText.text = "Name must be between 2 and 15 characters";
            return;
        }
        CreateCharacterButton[] buttons = characterUI.getCheckedButtons();
        Debug.Log("reference: " + characterUI + " : " + buttons + " : " + name);
        characterUI.getLogin().onCharacterCreated(buttons, name);

    }
}
