using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLightning : MonoBehaviour {

	LineRenderer line;
	List<Vector3> positions = new List<Vector3>();
	List<float> intervalsX = new List<float>();
	List<float> intervalsY = new List<float>();
	float range = 0.50f;
	void Start () {
		line = GetComponent<LineRenderer>();
		for(int i=0;i<line.positionCount;i++){
			positions.Add(line.GetPosition(i));
			intervalsX.Add(line.GetPosition(i).x);
		}
	}

	void Update () {
		/*foreach(Vector3 position in positions.ToArray()){
			position += new Vector3(Random.Range(0,1), Random.Range(0,1), Random.Range(0,1));
		}*/
		for(int i=0;i<line.positionCount;i++){
			line.SetPosition(i, new Vector3(Random.Range(-range,range), 0));
			//positions[i] += new Vector3(Random.Range(0,1), Random.Range(0,1), Random.Range(0,1));
			Debug.Log("randomizing");
		}
		line.SetPosition(0, new Vector3(0,0,0));
		line.SetPosition(11, new Vector3(0,0,0));
	}
	float choose(int num1, int num2){
		int rand = Random.Range(0,2);
		if(Mathf.RoundToInt(rand) == 0) return num1;
		if(Mathf.RoundToInt(rand) == 1) return num2;
		return 0;
	}
}
