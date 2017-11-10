using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPCManager;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

//5000 - 5500

public enum e_NpcTypes
{
    TALK,
    SHOP,
    QUESTGIVER
}

public enum e_DialogueLayouts
{
    OK,
    NEXT,
    YESNO
}
public class NPCMain : NetworkBehaviour {

    private Player player;
    private Text text;
    private int state = 0;
    private int selection = 0;
    private bool isTalking;


    public string[] dialogues;
    public string[,] keywords = {
        {"%playername%", "Kalle"},
        {"%playermoney%", "50"}

    }; 
    public bool hasQuest;
    private Quest quest;
    public e_NpcTypes type;
    private e_DialogueLayouts layout;



    private void Start()
    {
        this.text = GameObject.FindWithTag("DialogueText").GetComponent<Text>();

        for(int i = 0; i < dialogues.Length; i++) {
            for(int j=0;j<keywords.GetLength(0);j++){
                dialogues[i] = Regex.Replace(dialogues[i], @"\"+keywords[j,0]+"", keywords[j,1]);
            }
        }
        if (hasQuest) {
            quest = new Quest();
        }
    }

    private void Update()
    {
        if (isTalking) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                execute(state);
            }
        }
        if(state < dialogues.Length) {
            //YesNo or OK
            if(hasQuest) {
                layout = e_DialogueLayouts.YESNO;
            } else {
                layout = e_DialogueLayouts.OK;
            }

        } else {
            //Next
            layout = e_DialogueLayouts.NEXT;
        }
    }

    public void execute(int state)
    {
        Debug.Log(this.dialogues[state]);
        state++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player") && !isTalking){
            this.player = collision.gameObject.GetComponent<Player>();
            this.player.getPlayerMovement().freeze();
            isTalking = true;
        }
    }
}
 