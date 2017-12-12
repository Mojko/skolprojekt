using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : UIHandler {
    public Text text;
    InputField field;
    Player player;
    List<string> messages = new List<string>();
    int messagePos = 0;
	// Use this for initialization
	void Start () {
        base.initUI();
        //this.text = Tools.getChild(this.gameObject,"TextBox").GetComponent<Text>();
        this.field = Tools.getChild(this.gameObject, "InputChat").GetComponent<InputField>();
	}
    public void setPlayer(Player player) {
        this.player = player;
        Debug.Log("player set");
    }
    public bool isEnabled() {
        return isActive;
    }

    public void Update() {
        onMove();
        setFocused();

		if(canFocus()){
			this.player.getPlayerMovement().freeze();
		} else {
			this.player.getPlayerMovement().unfreeze();
		}

        if (field.IsActive() && field.text != "" && Input.GetKeyDown(KeyCode.Return)) {
            
            this.player.getNetwork().sendMessage(field.text,MessageTypes.CHAT,"");
            field.text = "";
            field.Select();
            field.ActivateInputField();
        }
    }
    public bool canClose() {
        if (field.IsActive()) return false;
        return true;
    }
    public InputField getInput() {
        return field;
    }
    public void addMessage(string message) {
        messages.Add(message);
        if (messages.Count > 20) {
            int index = text.text.IndexOf(System.Environment.NewLine);
            Debug.Log(index);
            text.text = text.text.Substring(messages[0].Length + 1);
            messages.RemoveAt(0);
        }
        text.text += message + "\n";
    }
    public void clear() {
        text.text = "";
    }
}
