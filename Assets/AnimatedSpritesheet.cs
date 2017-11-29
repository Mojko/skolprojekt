using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpritesheet : MonoBehaviour {

	public int fps;
	private Renderer particleSystemRender;
	private Sprite[] spritesheet;
	private float currentSprite;
	public string spritesheetName = "spritesheet_bite";
	public bool loop;

	// Use this for initialization
	void Start () {
		spritesheet = Resources.LoadAll<Sprite>(spritesheetName);
		//this.particleSystemRender = GetComponent<Renderer>();
		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.simulationSpeed = 15;
		/*particleSystemRender.material.SetTexture("_MainTex",Tools.spriteToTexture(spritesheet[0]));
		Debug.Log("SPRITESHEET___: " + spritesheet[0].texture + " | " + particleSystemRender);*/
	}
	
	// Update is called once per frame
	void Update () {
		/*currentSprite += (1*fps) * Time.deltaTime;
		if(currentSprite % 1 == 0){
			if((Mathf.RoundToInt(currentSprite)) < spritesheet.Length){
				particleSystemRender.material.SetTexture("_MainTex",Tools.spriteToTexture(spritesheet[Mathf.RoundToInt(currentSprite)]));
			} else {
				if(!loop){
					Destroy(this.gameObject);
				} else {
					currentSprite = 0;
				}
			}
		}*/
	}
}
