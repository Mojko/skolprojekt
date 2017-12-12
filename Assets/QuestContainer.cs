using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestContainer : MonoBehaviour {

	private Quest quest;
	private QuestInformationData questInformationData;
	private GameObject questInformationDescription;
	private GameObject questObjectiveWrapper;
	private GameObject questObjective;
	private Text questName;
	private Image thisImage;
	private ColorBlock colorBlock;
	private NPCController npcController;
	private NPCMain questGiver;

	private QuestWrapper questWrapper;
	[HideInInspector] public bool isClicked;
    private Button button;

	private List<Text> questObjectiveTexts = new List<Text>();
	private List<Image> questObjectiveImages = new List<Image>();

	private void Start(){
		questWrapper = this.GetComponentInParent<QuestWrapper>();
		questWrapper.questContainers.Add(this);
		thisImage = GetComponent<Image>();
        this.button = GetComponent<Button>();
	}

	public Quest getQuest(){
		return this.quest;
	}

	public void init(Quest quest){
		GameObject quest_UI = this.transform.parent.parent.parent.gameObject;
		questInformationData = quest_UI.transform.GetChild(0).GetComponent<QuestInformationData>();
		this.quest = quest;

		GameObject questInformation = this.transform.parent.parent.parent.GetChild(0).gameObject;
		this.questObjectiveWrapper = questInformation.transform.GetChild(questInformation.transform.childCount-1).gameObject;
		this.questName = questInformation.transform.GetChild(questInformation.transform.childCount-2).GetChild(1).GetComponent<Text>();
		Debug.Log("QNAME: " + questInformation.name);
		this.questInformationDescription = questInformation.transform.GetChild(1).gameObject;

		this.questObjective = questObjectiveWrapper.transform.GetChild(0).gameObject;
		RectTransform r = questObjective.GetComponent<RectTransform>();

		for(int i=0;i<quest.getCompletionData().completionId.Count;i++){
			GameObject o = Instantiate(questObjective);
			o.transform.SetParent(questObjectiveWrapper.transform);

			RectTransform rectTransform = o.GetComponent<RectTransform>();
			rectTransform.localPosition = r.localPosition;
			rectTransform.localPosition -= new Vector3(0, 40*i, 0);
			rectTransform.sizeDelta = r.sizeDelta;

			Image im = o.GetComponent<Image>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_MonsterIcons");
			if(sprites[quest.getImageIndex()] != null){
				im.sprite = sprites[quest.getImageIndex()];
			}
			im.enabled = false;
			Text t = o.GetComponentInChildren<Text>();
			t.enabled = false;

			this.questObjectiveImages.Add(im);
			this.questObjectiveTexts.Add(t);
		}
		this.npcController = GameObject.FindWithTag("NPCManager").GetComponent<NPCController>();
		this.questGiver = npcController.getNpcWithQuest(this.getQuest());
		Destroy(this.questObjective);
	}

	void Update(){
		if(!isClicked){
			thisImage.color = Tools.hexColor(0x6CB95D);
			toggleTextAndImages(false);
		}

		//Debug.Log("isClickedForMe: " + isClicked + " id: " + this.GetInstanceID());

	}

	void toggleTextAndImages(bool toggle){
		if(questObjectiveTexts.Count > 0){
			foreach(Text text in questObjectiveTexts){
				if(text != null) text.enabled = toggle;
			}
		}
		if(questObjectiveImages.Count > 0){
			foreach(Image image in questObjectiveImages){
				if(image != null) image.enabled = toggle;
			}
		}
	}

	public void setQuestInformation(){
		for(int i=0;i<this.questObjectiveTexts.Count;i++){
			if(this.questObjectiveTexts[i] != null){
				this.questObjectiveTexts[i].text = this.quest.getTooltip(i);
			}
		}
	}

	public void delete(){
		isClicked = false;
		for(int i=0;i<this.questObjectiveTexts.Count;i++){
			if(this.questObjectiveTexts[i] != null){
				this.questObjectiveTexts[i].text = "";
			}
		}
		this.questInformationDescription.GetComponent<Text>().text = "";
		npcController.updateSprite(null);
		this.questName.text = "";
		thisImage.color = Tools.hexColor(0x6CB95D);
		toggleTextAndImages(false);
		Destroy(this.gameObject);
	}

	public void onClick(){
		this.questWrapper.onQuestContainerClick();
		isClicked = true;
		toggleTextAndImages(true);
		setQuestInformation();
		thisImage.color = Tools.hexColor(0x599C4C);
		this.questInformationDescription.GetComponent<Text>().text = this.quest.getDescription();
		this.questName.text = this.quest.getName();
		if(this.questGiver != null){
			npcController.updateSprite(this.questGiver.getSprite());
		}
		this.questInformationData.toggleActive(true, this);
	}
}