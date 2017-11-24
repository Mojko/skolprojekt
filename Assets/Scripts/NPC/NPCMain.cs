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
	private bool canTalk = true;

	public int npcId;
    public string[] dialogues;
	public string[,] keywords;
	public int[] questIds;
	private List<Quest> questsCompleted = new List<Quest>();
    public e_NpcTypes type;
    private e_DialogueLayouts layout;
	private bool shouldInitilize = true;
	private Sprite faceImage;
	public Text dialogue;
	public GameObject dialogueUI;

    [Space(10)]
    [Header("Leave these alone")]
    public GameObject questMark;




    private void Start()
    {
        //this.text = GameObject.FindWithTag("DialogueText").GetComponent<Text>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_NpcIcons");
		Debug.Log("defaultids: " + DefaultIds.getNpcDefault());
		faceImage = sprites[(npcId/DefaultIds.getNpcDefault())-1];
        this.questMark = transform.GetChild(0).gameObject;
		if(faceImage == null)
			faceImage = sprites[0];
    }

	public Sprite getSprite(){
		return this.faceImage;
	}

	private void dispose(){
        isTalking = false;
		dialogueUI.SetActive(false);
		this.player.getPlayerMovement().unfreeze();
		this.player = null;
        this.state = 0;
	}

    private void Update()
    {
        if (isTalking) {
			if (Input.GetMouseButtonDown(0)) {
                execute();
				if(layout == e_DialogueLayouts.YESNO || layout == e_DialogueLayouts.OK) canTalk = false;

				if(layout == e_DialogueLayouts.YESNO){ 
					Quest q = this.player.lookupQuest(questIds[questsCompleted.Count]);
                    //Quest q = new Quest(questIds[questsCompleted.Count], this.player.getCharacterName());
                    if(q == null) q = new Quest(questIds[questsCompleted.Count], this.player.getCharacterName());
					if(this.player.canTakeQuest(q)){
                        Debug.Log("status of quest: " + q.getStatus());
						giveQuestToPlayer(q);
                        this.questMark.SetActive(false);
					}
					dispose();
					//No more quests to give out
				}
            }
        }
        if(state >= dialogues.Length) {
            //YesNo or OK
			if(questIds.Length >= questsCompleted.Count) {
                layout = e_DialogueLayouts.YESNO;
            } else {
                layout = e_DialogueLayouts.OK;
            }

        } else {
            //Next
            layout = e_DialogueLayouts.NEXT;
        }
		/*if(player != null){
			for(int i=0;i<questIds.Length;i++){
				for(int j=0;j<player.getQuests().Length;j++){
					if(player.getQuests()[j].getStatus() == e_QuestStatus.COMPLETED){
						Debug.Log("QUEST IS COMPLETED: " + player.getQuests()[j].getId());
					}
				}
			}
		}*/
    }

    public void execute()
    {
		if(state < dialogues.Length){
			dialogue.text = this.dialogues[state];
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

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Collliding, isTalking: " + isTalking + " | canTalk: " + canTalk);
		if(collision.gameObject.CompareTag("Player") && canTalk && !isTalking && Input.GetMouseButton(0)){
            this.player = collision.gameObject.GetComponent<Player>();
            this.player.getPlayerMovement().freeze();
            isTalking = true;
			dialogueUI.SetActive(true);

			if(shouldInitilize){
				init();
			}
        }
    }

	private void OnCollisionExit(Collision collision){
		canTalk = true;
	}
}
  