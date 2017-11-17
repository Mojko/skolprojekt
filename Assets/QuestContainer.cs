using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestContainer : MonoBehaviour {
	
	private QuestInformationData questInformationData;
	private Quest quest;

	private GameObject questInformationDescription;
	private GameObject questObjectiveWrapper;
	private GameObject questObjective;
	private Text questName;
	private Image thisImage;
	private ColorBlock colorBlock;

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
			Debug.Log("I: " + i);
			rectTransform.localPosition -= new Vector3(0, 40*i, 0);
			rectTransform.sizeDelta = r.sizeDelta;

			Image im = o.GetComponent<Image>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_MonsterIcons");
            im.sprite = sprites[0];
			im.enabled = false;
			Text t = o.GetComponentInChildren<Text>();
			t.enabled = false;

			this.questObjectiveImages.Add(im);
			this.questObjectiveTexts.Add(t);
		}
		Destroy(this.questObjective);
	}

	void Update(){
		if(!isClicked){
			toggleTextAndImages(false);
		} else {
            this.button.Select();
        }
	}

	void toggleTextAndImages(bool toggle){
		foreach(Text text in questObjectiveTexts){
			text.enabled = toggle;
		}
		foreach(Image image in questObjectiveImages){
			image.enabled = toggle;
		}
	}

	public void setQuestInformation(){
		for(int i=0;i<this.questObjectiveTexts.Count;i++){
			this.questObjectiveTexts[i].text = this.quest.getTooltip(i);
		}
	}

	public void onClick(){
		isClicked = true;
		this.questWrapper.onQuestContainerClick();
		toggleTextAndImages(true);
		setQuestInformation();
		this.questInformationDescription.GetComponent<Text>().text = this.quest.getDescription();
		this.questName.text = this.quest.getName();
	}
}
