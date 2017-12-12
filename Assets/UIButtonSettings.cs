using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSettings : MonoBehaviour {

	public void onClick(GameObject obj){
		GameObject clone = Tools.findInactiveChild(this.transform.root.gameObject, obj.name + "(Clone)");
		if(clone != null){
			clone.SetActive(!clone.activeInHierarchy);
		} else {
			obj.SetActive(!obj.activeInHierarchy);
		}
	}
}
