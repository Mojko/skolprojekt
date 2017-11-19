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


	public int npcId;
    public string[] dialogues;
	public string[,] keywords;
	public int[] questIds;
	private List<Quest> questsCompleted = new List<Quest>();
    public e_NpcTypes type;
    private e_DialogueLayouts layout;
	private bool shouldInitilize = true;
	private Sprite faceImage;




    private void Start()
    {
        //this.text = GameObject.FindWithTag("DialogueText").GetComponent<Text>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_NpcIcons");
		faceImage = sprites[npcId/DefaultIds.getNpcDefault()];
		if(faceImage == null)
			faceImage = sprites[0];
    }

	public Sprite getSprite(){
		return this.faceImage;
	}

    private void Update()
    {
        if (isTalking) {
			if (Input.GetMouseButtonDown(0)) {
                execute();
				if(layout == e_DialogueLayouts.YESNO){ 
					Quest q = new Quest(questIds[questsCompleted.Count], this.player.getCharacterName());
					if(!this.player.hasQuest(q)){
						giveQuestToPlayer(q); 
					}
					q = null;
					isTalking = false; 
					return;
				}
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
		if(player != null){
			for(int i=0;i<questIds.Length;i++){
				for(int j=0;j<player.getQuests().Length;j++){
					if(player.getQuests()[j].getStatus() == e_QuestStatus.COMPLETED){
						Debug.Log("QUEST IS COMPLETED: " + player.getQuests()[j].getId());
					}
				}
			}
		}
    }

    public void execute()
    {
		if(state < dialogues.Length){
	        Debug.Log(this.dialogues[state]);
	        state++;
		}
    }

	private void giveQuestToPlayer(Quest quest){
		Debug.Log("wew: " + questIds.Length + " | " + this.player.getQuests().Length);
		this.player.getNetwork().sendQuestToServer(quest);
		Debug.Log("Quest has been assigned");
		/*if (questIds != null) {
            if(this.player.getQuests().Length > 0){
			    for(int i=0;i<questIds.Length;i++){
					if(this.player.hasQuest())
					    this.player.getNetwork().sendQuestToServer(new Quest(questIds[i], this.player.getCharacterName()));
					    Debug.Log("Quest has been assigned");
					    return;
				    }
			    }
            } else {
                this.player.getNetwork().sendQuestToServer(new Quest(questIds[0], this.player.getCharacterName()));
            }
		}*/
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
 