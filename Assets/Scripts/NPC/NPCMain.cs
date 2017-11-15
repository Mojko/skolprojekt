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
	public string[,] keywords;
	public int[] questIds;
    public e_NpcTypes type;
    private e_DialogueLayouts layout;
	private bool shouldInitilize = true;




    private void Start()
    {
        //this.text = GameObject.FindWithTag("DialogueText").GetComponent<Text>();
    }

    private void Update()
    {
        if (isTalking) {
			if (Input.GetKeyDown(KeyCode.Q)) {
                execute();
				if(layout == e_DialogueLayouts.YESNO) giveQuestToPlayer();
            }
        }
        if(state >= dialogues.Length) {
            //YesNo or OK
			if(questIds != null) {
                layout = e_DialogueLayouts.YESNO;
            } else {
                layout = e_DialogueLayouts.OK;
            }

        } else {
            //Next
            layout = e_DialogueLayouts.NEXT;
        }
    }

    public void execute()
    {
		if(state < dialogues.Length){
	        Debug.Log(this.dialogues[state]);
	        state++;
		}
    }

	private void giveQuestToPlayer(){
        Debug.Log("wew: " + questIds.Length + " | " + this.player.getQuests());
		if (questIds != null) {
            if(this.player.getQuests().Length > 0){
			    foreach(Quest quest in this.player.getQuests()){
				    for(int i=0;i<questIds.Length;i++){
					    if(quest.getId() == questIds[i]){
						    this.player.getNetwork().sendQuestToServer(new Quest(questIds[i], this.player.getCharacterName()));
						    Debug.Log("Quest has been assigned");
						    return;
					    }
				    }
			    }
            } else {
                this.player.getNetwork().sendQuestToServer(new Quest(questIds[0], this.player.getCharacterName()));
            }
		}
	}

	private void init(){
		this.keywords = new string[,] {
			{"%playername%", player.getCharacterName()},
			{"%playermoney%", player.money.ToString()}
		}; 
		for(int i = 0; i < dialogues.Length; i++) {
			for(int j=0;j<keywords.GetLength(0);j++){
				dialogues[i] = Regex.Replace(dialogues[i], @"\"+keywords[j,0]+"", keywords[j,1]);
			}
		}
		this.shouldInitilize = false;
	}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player") && !isTalking){
            this.player = collision.gameObject.GetComponent<Player>();
            this.player.getPlayerMovement().freeze();
            isTalking = true;

			if(shouldInitilize){
				init();
			}
        }
    }
}
 