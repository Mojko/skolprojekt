using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpritesheet : MonoBehaviour {

	public int fps;
	private Renderer particleSystemRender;
	private Sprite[] spritesheet;
	private float currentSprite;

	// Use this for initialization
	void Start () {
		spritesheet = Resources.LoadAll<Sprite>("spritesheet_bite");
		this.particleSystemRender = GetComponent<Renderer>();

		particleSystemRender.material.SetTexture("_MainTex",Tools.spriteToTexture(spritesheet[0]));
		Debug.Log("SPRITESHEET___: " + spritesheet[0].texture + " | " + particleSystemRender);
	}
	
	// Update is called once per frame
	void Update () {
		currentSprite += (1*fps) * Time.deltaTime;
		if((Mathf.RoundToInt(currentSprite)) < spritesheet.Length){
			particleSystemRender.material.SetTexture("_MainTex",Tools.spriteToTexture(spritesheet[Mathf.RoundToInt(currentSprite)]));
		} else {
			Destroy(this.gameObject);
		}
	}
}
