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
	private List<Quest> questsCompleted = new List<Quest>();
    private e_DialogueLayouts layout;
	private bool shouldInitilize = true;
	private Sprite faceImage;

	[Header("NPC")]
	[Space(10)]
	public int npcId;
	public e_NpcTypes type;
	public string[] dialogues;
	public string[,] keywords;
	public int[] questIds;
	public string[] secretDialogues;
	public string[] returnDialogues;

	[Header("System")]
	[Space(10)]
	public Text dialogue;
	public GameObject dialogueUI;

	[Header("Special Effects")]
	[Space(10)]
	public Color goldColor;
	public Color silverColor;

    [Header("Leave these alone")]
	[Space(10)]
    public GameObject exclamationMark;
	private Renderer[] exclamationMarkRenderers;
	public GameObject questionMark;
	private Renderer[] questionMarkRenderers;





    private void Start()
    {
        //this.text = GameObject.FindWithTag("DialogueText").GetComponent<Text>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_NpcIcons");
		faceImage = sprites[(npcId/DefaultIds.getNpcDefault())-1];

		this.exclamationMark = transform.GetChild(0).gameObject;
		this.questionMark = transform.GetChild(1).gameObject;

		this.exclamationMarkRenderers = exclamationMark.GetComponentsInChildren<Renderer>();
		this.questionMarkRenderers = questionMark.GetComponentsInChildren<Renderer>();

		if(faceImage == null)
			faceImage = sprites[0];
    }

	public int getAcessabilityForDialogues(){
		int levelToAcess = 0;
		for(int i=0;i<secretDialogues.Length;i++){
			foreach(Quest q in questsCompleted){
				if(q.getId().Equals(secretDialogues[i])){
					levelToAcess = i;
				}
			}
		}
		return levelToAcess;
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

	public bool hasQuest(Quest quest){
		for(int i=0;i<questIds.Length;i++){
			return quest.getId() == questIds[i];
		}
		return false;
	}

	public void setQuestMarker(Player player){
		foreach(Quest q in player.getQuests()){
			if(q.getStatus() == e_QuestStatus.COMPLETED){
				setQuestionMarkCompleted();
			} else if(q.getStatus() == e_QuestStatus.STARTED){
				setQuestionMarkPending();
			}
		}
		int j = 0;
		for(int i=0;i<this.questIds.Length;i++){
			if(player.hasQuest(questIds[i])){
				j++;
			}
		}
		if(j <= 0){
			setExclamationMarkHasQuest();
		}
	}

	void setQuestionMarkCompleted(){
		this.questionMark.SetActive(true);
		foreach(Renderer r in this.questionMarkRenderers){
			r.material.color = this.goldColor;
		}
		setExclamationMarkDisabled();
	}
	void setQuestionMarkPending(){
		this.questionMark.SetActive(true);
		foreach(Renderer r in this.questionMarkRenderers){
			r.material.color = this.silverColor;
		}
	}
	void setQuestionMarkDisabled(){
		this.questionMark.SetActive(false);
	}
	void setExclamationMarkHasQuest(){
		this.exclamationMark.SetActive(true);
		foreach(Renderer r in this.exclamationMarkRenderers){
			r.material.color = this.goldColor;
		}
		setQuestionMarkDisabled();
	}
	void setExclamationMarkDisabled(){
		this.exclamationMark.SetActive(false);
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
						this.exclamationMark.SetActive(false);
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

	public string getDialogueAfterQuestId(string[] dialogue, int questId){
		for(int i=0;i<dialogue.Length;i++){
			if(dialogue[i].Equals(questId)){
				return dialogue[i];
			}
		}
		return "";
	}

    public void execute()
    {
		for(int i=0;i<this.questIds.Length;i++){
			if(player.hasQuest(questIds[i]) && !player.hasTurnedInQuest(player.lookupQuest(questIds[i]))){
				dialogue.text = getDialogueAfterQuestId(returnDialogues, questIds[i]);
				return;
			}
		}

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
  