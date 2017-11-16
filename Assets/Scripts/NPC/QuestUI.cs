using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour {

	/*public GameObject questContainerPrefab;
	public GameObject questPrefab;

	public GameObject questToolTipPrefab;

	public GameObject questHelperPrefab;
	private GameObject questHelperObject;

	private float questPrefabyOffset = 0;
	private int amountOfQuestPanels = 0;
	private int amountOfToolTips = 0;

	private GameObject questContainerObject;

	void Start () {
		this.questContainerObject = this.transform.Find("Panel").Find(questContainerPrefab.name).gameObject;
		this.questHelperObject = this.transform.root.Find(questHelperPrefab.name).gameObject;
		questPrefabyOffset = questPrefab.transform.position.y;
	}



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
	}

	public void removeToolTip(){
		amountOfToolTips--;
	}
	*/
}
