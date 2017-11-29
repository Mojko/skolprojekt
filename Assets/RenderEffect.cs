using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderEffect : MonoBehaviour {

	Shader shader;

	void OnEnable(){
		this.GetComponent<Camera>().SetReplacementShader(shader, "");
	}

}
