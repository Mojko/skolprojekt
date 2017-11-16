using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInformationData : MonoBehaviour {

	[Header("Quests")]
    public GameObject npcObject;
    public GameObject questWrapper;
    public GameObject questContainer;
	public Text questDescriptionText;

	[Header("QuestObjective(leave empty)")]
	public GameObject questObjectiveWrapper;
	private List<Text> questObjectiveTexts = new List<Text>();
	private List<Image> questObjectiveImages = new List<Image>();
	public GameObject questObjectiveGameObject;
    
    private Image npcImage;
    
	private Dictionary<int, GameObject> questPanels = new Dictionary<int, GameObject>();


	void Start () {
        this.npcImage = npcObject.GetComponent<Image>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_MonsterIcons"); // Fix l8er
        //this.questObjectiveImage = questObjectiveGameObject.GetComponent<Image>();
        //this.questObjectiveText = questObjectiveGameObject.GetComponentInChildren<Text>();
		this.transform.SetAsFirstSibling();
	}

    public void addNewQuestPanel(Quest quest)
    {
        GameObject tempQuestPanel = Instantiate(this.questContainer);
		tempQuestPanel.transform.SetParent(this.questWrapper.transform);
		tempQuestPanel.GetComponentInChildren<Text>().text = quest.getName();

		/*this.questObjectiveWrapper = this.transform.GetChild(this.transform.childCount-1).gameObject;//this.transform.parent.GetChild(this.transform.parent.childCount-1).gameObject;
		this.questObjectiveGameObject = questObjectiveWrapper.transform.GetChild(0).gameObject;
		RectTransform r = questObjectiveGameObject.GetComponent<RectTransform>();

		for(int i=0;i<quest.getCompletionData().completionId.Count;i++){
			GameObject o = Instantiate(this.questObjectiveGameObject);
			o.transform.SetParent(this.questObjectiveWrapper.transform);

			RectTransform rectTransform = o.GetComponent<RectTransform>();
			rectTransform.localPosition = r.localPosition;
			Debug.Log("I: " + i);
			rectTransform.localPosition -= new Vector3(0, 40*i, 0);
			rectTransform.sizeDelta = r.sizeDelta;

			Image im = o.GetComponent<Image>();
			im.enabled = false;
			Text t = o.GetComponentInChildren<Text>();
			t.enabled = false;

			this.questObjectiveImages.Add(im);
			this.questObjectiveTexts.Add(t);
		}*/
		//this.questObjectiveImage = questObjectiveGameObject.GetComponent<Image>();
		//this.questObjectiveText = questObjectiveGameObject.transform.GetChild(0).GetComponent<Text>();
		//Debug.Log("TEXt: " + questObjectiveText.text);

		RectTransform rect = tempQuestPanel.GetComponent<RectTransform>();
        rect.anchoredPosition = this.questContainer.GetComponent<RectTransform>().anchoredPosition;
		rect.localPosition = new Vector2(0,170+(rect.sizeDelta.y*-questPanels.Count));
		rect.sizeDelta = new Vector2(0,50);

		tempQuestPanel.GetComponent<QuestContainer>().init(quest);
		questPanels.Add(quest.getId(), tempQuestPanel);

    }

	public void removeQuestPanel(Quest quest){
		questPanels.Remove(quest.getId());
	}

    /*
	public void addNewQuestPanel(Quest quest){
		GameObject questObject = Instantiate(questPrefab);
		questObject.transform.SetParent(this.questContainerObject.transform);

		RectTransform rect = questObject.GetComponent<RectTransform>();
		rect.anchoredPosition = questPrefab.transform.position;
		rect.anchoredPosition -= new Vector2(0,questPrefabyOffset*amountOfQuestPanels);

		amountOfQuestPanels++;
	}

	public void removeQuestPanel(){
		amountOfQuestPanels--;
	}

	public void addNewQuestToolTip(string text){
		GameObject toolTip = Instantiate(questToolTipPrefab);
		toolTip.transform.SetParent(this.questHelperObject.transform);

		toolTip.GetComponent<Text>().text = text;

		RectTransform rect = toolTip.GetComponent<RectTransform>();
		rect.anchoredPosition = questPrefab.transform.position;
		rect.anchoredPosition -= new Vector2(0,questPrefabyOffset * amountOfToolTips);
		amountOfToolTips++;
		Debug.Log("hi");
	}*/
}

