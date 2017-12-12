using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPCManager;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;


[System.Serializable]
public class DialogueMain {
	public string[] dialogues;
	public Quest[] questsToAcess;
	public Quest[] questsToGiveOut;
	private int state = 0;
	private bool finished;
	private int id;

	public void setId(int id){
		this.id = id;
	}
	public int getId(){
		return this.id;
	}


	public string execute(){

		if(dialogues.Length-1 == state){
			finished = true;
		}

		if(dialogues.Length > state){
			string dialogueToReturn = dialogues[state];
			state++;
			return dialogueToReturn;
		}
		return "Error";
	}

	public void dispose(){
		this.state = 0;
		this.finished = false;
	}

	public bool isFinished(){
		return this.finished;
	}

	public Quest canAcess(Quest[] quests){

		if(this.questsToAcess.Length <= 0) return null;

		foreach(Quest playerQuest in quests){
			foreach(Quest dialogueQuest in this.questsToAcess){
				if(playerQuest.getId() == dialogueQuest.getId()){
					if(playerQuest.getStatus() == dialogueQuest.getStatus()){
						return playerQuest;
					}
				}
			}
		}
		return null;
	}
}


public class NPCMain : NetworkBehaviour {

	public string nickname;
	public DialogueMain[] dialogues;
	public Quest[] quests;


	private Player player;
	private int index;
	private DialogueMain currentDialoguePlaying;
	private bool canTalk = true;
	private bool isTalking = false;
	private Animator animator;

	[HideInInspector]public int[] questIds;
	[HideInInspector]public Text dialogueUI_questDescription;
	[HideInInspector]public Text dialogueUI_questGiverName;
	[HideInInspector]public Image dialogueUI_questGiverFace;
	[HideInInspector]public GameObject dialogueUI;
	[HideInInspector]public int uniqueId;
	[HideInInspector]public Sprite faceImage;

	[Header("System")]
	[Space(10)]
	public GameObject exclamationMark;
	private Renderer[] exclamationMarkRenderers;
	public GameObject questionMark;
	private Renderer[] questionMarkRenderers;
	public Color goldColor;
	public Color silverColor;

	private void Start(){
		this.gameObject.SetActive(true);
		this.animator = GetComponent<Animator>();
		this.exclamationMarkRenderers = exclamationMark.GetComponentsInChildren<Renderer>();
		this.questionMarkRenderers = questionMark.GetComponentsInChildren<Renderer>();
	}

	private void Update(){

		Collider[] col = Physics.OverlapSphere(this.transform.position, 1);
		foreach(Collider c in col){
			if(c.gameObject.CompareTag("Player")){
				if(currentDialoguePlaying == null && canTalk && !isTalking){
					if(player == null){
						this.player = c.gameObject.GetComponent<Player>();
						this.player.npcTalkingTo = this.gameObject;
					}
					talk();
				}
			}
		}

		if(currentDialoguePlaying != null && Input.GetKeyDown(KeyCode.Z)){
			if(!currentDialoguePlaying.isFinished()){
				
				if(dialogueUI.activeInHierarchy == false){
					dialogueUI.SetActive(true);
					dialogueUI_questGiverName.text = this.nickname;
					dialogueUI_questGiverFace.sprite = (Resources.LoadAll<Sprite>("spritesheet_NpcIcons"))[this.uniqueId];
					this.player.getPlayerMovement().freeze();
				}

				this.dialogueUI_questDescription.text = currentDialoguePlaying.execute();
				if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Talk")){
					animator.Play("Talk");
				}
				return;
			} else {
				Debug.Log("QUESTS_LENGTH: " + quests.Length + " | " + currentDialoguePlaying.getId());

				foreach(Quest q in this.currentDialoguePlaying.questsToGiveOut){
					if(!player.hasQuest(q)){
						giveQuestToPlayer(new Quest(q.id, player.getCharacterName()));
						dispose();
						return;
					}
				}

				/*for(int i=0;i<this.quests.Length;i++){
					if(this.quests[i].getStatus() == e_QuestStatus.COMPLETED){
						onQuestTurnIn(this.quests[i]);
						dispose();
						return;
					}
				}*/
				if(player != null){
					foreach(Quest q in this.player.quests){
						if(q.getStatus() == e_QuestStatus.COMPLETED){
							this.player.getNetwork().sendQuestToServer(q, PacketTypes.QUEST_TURN_IN);
							dispose();
							return;
						}
					}
				}
			}
			dispose();
		}
	}

	public void dispose(){
		this.player.getPlayerMovement().unfreeze();
		if(currentDialoguePlaying != null){
			this.currentDialoguePlaying.dispose();
			currentDialoguePlaying = null;
		}
		dialogueUI.SetActive(false);
		isTalking = false;
		canTalk = false;

		if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
			animator.Play("Idle");
		}
	}

	public void talk(){
		
		for(int i=dialogues.Length-1;i>=0;i--){
			
			bool skipFirst = false;
			foreach(Quest q in dialogues[i].questsToAcess){
				Debug.Log("FIRST_DIALOGUE: " + dialogues[i].dialogues[0] + " | " + q.getStatus() + " | " + q.getId());
				Quest playerq = player.lookupQuest(q.getId());
				if(playerq != null){
					if(playerq.getStatus() != e_QuestStatus.TURNED_IN){
						skipFirst = true;
						continue;
					}
				}
				if(q.getStatus() == e_QuestStatus.NOT_STARTED){
					//skipFirst = true;
					//continue;
				} else {
					if(playerq != null){
						if(playerq.getStatus() != q.getStatus()){
							skipFirst = true;
							continue;
						}
					} else {
						skipFirst = true;
						continue;
					}
				}
			}
			if(skipFirst) continue;

			currentDialoguePlaying = dialogues[i];
			currentDialoguePlaying.setId(i);
			canTalk = false;
			isTalking = true;
			Debug.Log("REAL_DIALOGUE: " + dialogues[i].dialogues[0] + " | "  + currentDialoguePlaying);
			return;
		}
	}
	public void giveQuestToPlayer(Quest quest){
		if(!player.hasQuest(quest)){
			player.getNetwork().sendQuestToServer(quest, PacketTypes.QUEST_START);
		}
		Debug.Log("quest has been assigned");
	}
	private void OnCollisionStay(Collision collision)
	{
		/*if(collision.gameObject.CompareTag("Player") && currentDialoguePlaying == null && canTalk && !isTalking){
			if(player == null){
				this.player = collision.gameObject.GetComponent<Player>();
				this.player.npcTalkingTo = this.gameObject;
			}
			talk();
			return;
		}*/
	}

	private void OnTriggerExit(Collider collision){
		isTalking = false;
		dispose();
		canTalk = true;
	}

	public Sprite getSprite(){
		return this.faceImage;
	}
	public void onQuestTurnIn(Quest q){
		questCompletedEffect(q);
		//this.player.turnInQuest(q);
		dispose();
	}
	public void stopTalking(){
	}
	public void questCompletedEffect(Quest q)
	{
		GameObject o = (GameObject)Instantiate(Resources.Load("SpecialEffects/QuestTurnedIn"));
		o.transform.position = this.player.transform.position;
		ParticleScaler s = o.GetComponent<ParticleScaler>();
		s.objectAttachedTo = this.player.gameObject;

		if(s.levelUpUI != null){
			Text t = s.levelUpUI.transform.Find("Panel").Find("QuestName").GetComponent<Text>();
			t.text = "Quest Completed " + q.getName();
		}
	}






	//SPaghetti codeieo


	public void setQuestMarker(Player player){
		foreach(Quest q in player.getQuests()){
			if(q.getStatus() == e_QuestStatus.COMPLETED){
				setQuestionMarkCompleted();
				return;
			} else if(q.getStatus() == e_QuestStatus.STARTED){
				setQuestionMarkPending();
				return;
			}
		}

		int j = 0;
		foreach(Quest q in this.quests){
			if(player.hasQuest(q)){
				j++;
			}
		}
		if(j <= 0){
			setExclamationMarkHasQuest();
			return;
		}
		setQuestionMarkDisabled();
	}
	public void setQuestMarker(Quest quest){
		Debug.Log("QUEST COMPLETION: " + quest.getStatus().ToString());
		if(quest.getStatus() == e_QuestStatus.COMPLETED){
			setQuestionMarkCompleted();
			return;
		} else if(quest.getStatus() == e_QuestStatus.STARTED){
			setQuestionMarkPending();
			return;
		}

		int j = 0;
		foreach(Quest q in this.quests){
			if(player.hasQuest(q)){
				j++;
			}
		}
		if(j <= 0){
			setExclamationMarkHasQuest();
			return;
		}
		setQuestionMarkDisabled();
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
			return;
		}
		setQuestionMarkDisabled();
	}
	void setExclamationMarkDisabled(){
		this.exclamationMark.SetActive(false);
	}


}





















/*
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

	private Dialogue d = new Dialogue();
    private Player player;
    private Text text;
    private int state = 0;
    private int selection = 0;
    private bool isTalking;
	private bool canTalk = true;
	public List<Quest> questsCompleted = new List<Quest>();
    private e_DialogueLayouts layout;
	private bool shouldInitilize = true;
	private Sprite faceImage;

	[Header("NPC")]
	[Space(10)]
	public int npcId;
	public e_NpcTypes type;
	public string[] dialogues;
	public string[,] keywords;
	public Quest[] quests;
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
		Debug.Log("defaultids: " + DefaultIds.getNpcDefault());
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
        this.state = 0;
	}

	public bool hasQuest(Quest quest){
		foreach(Quest q in quests){
			return (quest.getId() == q.getId());
		}
		return false;
	}
	/*



	 
	public void setQuestMarker(Player player){
		foreach(Quest q in player.getQuests()){
			if(q.getStatus() == e_QuestStatus.COMPLETED){
				setQuestionMarkCompleted();
			} else if(q.getStatus() == e_QuestStatus.STARTED){
				setQuestionMarkPending();
			}
		}

		int j = 0;
		foreach(Quest q in this.quests){
			if(player.hasQuest(q)){
				j++;
			}
		}
		if(j <= 0){
			setExclamationMarkHasQuest();
			return;
		}
		setQuestionMarkDisabled();
	}
	public void setQuestMarker(Quest quest){
		Debug.Log("QUEST COMPLETION: " + quest.getStatus().ToString());
		if(quest.getStatus() == e_QuestStatus.COMPLETED){
			setQuestionMarkCompleted();
			return;
		} else if(quest.getStatus() == e_QuestStatus.STARTED){
			setQuestionMarkPending();
			return;
		}
			
		int j = 0;
		foreach(Quest q in this.quests){
			if(player.hasQuest(q)){
				j++;
			}
		}
		if(j <= 0){
			setExclamationMarkHasQuest();
			return;
		}
		setQuestionMarkDisabled();
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
			return;
		}
		setQuestionMarkDisabled();
	}
	void setExclamationMarkDisabled(){
		this.exclamationMark.SetActive(false);
	}


	public void onQuestTurnIn(Quest quest){
		foreach(Quest q in this.questsCompleted.ToArray()){
			if(q.getId() == quest.getId()){
				return;
			}
		}
		this.questsCompleted.Add(quest);
		setQuestMarker(quest);
		Debug.Log("TURNED IN QUEST");
	}

    private void Update()
    {
        if (isTalking) {
			if (Input.GetMouseButtonDown(0)) {
                execute();


				int index = questsCompleted.Count-1;
				index = Mathf.Clamp(index, 0, 1000);
				Quest q = this.player.lookupQuest(questIds[index]);
				if(q == null) q = new Quest(questIds[index], this.player.getCharacterName());

				if(q.getStatus() == e_QuestStatus.COMPLETED){
					this.player.getNetwork().sendQuestToServer(q, PacketTypes.QUEST_TURN_IN);
				}

				if(this.player.canTakeQuest(q)){
                    Debug.Log("status of quest: " + q.getStatus());
					giveQuestToPlayer(q);
					this.exclamationMark.SetActive(false);
				}

				dispose();
				//No more quests to give out
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
    }

	public string getDialogueAfterQuestId(string[] dialogue, int questId){
		for(int i=0;i<dialogue.Length;i++){
			Debug.Log("=======DIALGOUE: " + dialogue[i]);
			if(dialogue[i].Equals(questId)){
				return dialogue[i];
			}
		}
		return "";
	}

    public void execute()
    {
		if(state >= this.dialogues.Length){
			for(int i=0;i<this.questIds.Length;i++){
				if(player.hasQuest(questIds[i]) && !player.hasTurnedInQuest(player.lookupQuest(questIds[i]))){
					dialogue.text = getDialogueAfterQuestId(returnDialogues, questIds[i]);
					return;
				}
			}
		}

		if(state < dialogues.Length){
			dialogue.text = this.dialogues[state];
	        Debug.Log(this.dialogues[state]);
	        state++;
		} else {
			foreach(Quest q in this.quests){
				if(!player.hasQuest(q)){
					giveQuestToPlayer(q);
					break;
				}
			}
		}
    }

	private void giveQuestToPlayer(Quest quest){
		Debug.Log("wew: " + questIds.Length + " | " + this.player.getQuests().Length);
		this.player.getNetwork().sendQuestToServer(quest, PacketTypes.QUEST_START);
		Debug.Log("Quest has been assigned");
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
		if(collision.gameObject.CompareTag("Player") && canTalk && !isTalking && Input.GetMouseButtonDown(0)){
            this.player = collision.gameObject.GetComponent<Player>();
            this.player.getPlayerMovement().freeze();
			this.player.npcTalkingTo = this.gameObject;
            isTalking = true;
			dialogueUI.SetActive(true);

			if(shouldInitilize){
				init();
			}
			execute();
        }
    }

	private void OnCollisionExit(Collision collision){
		canTalk = true;
		this.player = null;
	}
}
 */