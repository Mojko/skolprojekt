using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInformationData : MonoBehaviour {

    public GameObject npcObject;
    public GameObject questObjectiveGameObject;
    public GameObject questWrapper;
    public GameObject questContainer;
    public Text questDescriptionText;

    private Text questObjectiveText;
    private Image npcImage;
    private Image questObjectiveImage;


	void Start () {
        this.npcImage = npcObject.GetComponent<Image>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheet_MonsterIcons"); // Fix l8er
        //this.questObjectiveImage = questObjectiveGameObject.GetComponent<Image>();
        //this.questObjectiveText = questObjectiveGameObject.GetComponentInChildren<Text>();
	}

    public void addNewQuestPanel(Quest quest)
    {
        GameObject tempQuestPanel = Instantiate(this.questContainer);

        RectTransform rect = tempQuestPanel.GetComponent<RectTransform>();
        rect.anchoredPosition = this.questContainer.GetComponent<RectTransform>().anchoredPosition;

        tempQuestPanel.transform.SetParent(this.questWrapper.transform);
        tempQuestPanel.GetComponentInChildren<Text>().text = quest.getName();
        rect.localPosition = new Vector2(0,120);
        rect.sizeDelta = new Vector2(0,50);
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
