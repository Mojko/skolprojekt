using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour {

	private string[,] keywords;

	float r;
	float g;
	float b;
	Text text;
	float a = 1;
	[HideInInspector] public float speed = 10f;

	void Start(){
		this.text = this.GetComponent<Text>();
		this.r = this.text.color.r;
		this.g = this.text.color.g;
		this.b = this.text.color.b;

		this.keywords = new string[,] {
			{"%typeString%", ""},
			{"%current%", ""},
			{"%requirement%", ""},
			{"%type%", ""}
		};

	}

	void Update () {
		this.GetComponent<RectTransform>().anchoredPosition += new Vector2(0,speed * Time.deltaTime);
		if(a > 0){
			a -= 0.7f * Time.deltaTime;
		} else {
			//this.transform.root.Find("Quest_UI").GetComponent<QuestUI>().removeToolTip();
			Destroy(this.gameObject);
		}
		this.text.color = new Color(r,g,b,a);
	}
}
