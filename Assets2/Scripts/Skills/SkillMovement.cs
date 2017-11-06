using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMovement : MonoBehaviour {

    private void Update () 
    {
        this.transform.position += transform.forward * 4 * Time.deltaTime;
	}
}
