using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_Faces
{
    DEFAULT,
    ANGRY,
	ATTACK,
    TALKING
}

public class FaceManager : MonoBehaviour {
	
    public Texture[] faces;
    public e_Faces[] faceTypes;
	private e_Faces currentFace;

    public void setFace(Renderer renderer, e_Faces face)
    {
        int index = (int)face;
		currentFace = face;
        Debug.Log("FACE: " + face.ToString() + " | " + faces[index]);
        if(faceTypes[index].Equals(face)){
            renderer.material.SetTexture("_MainTex", faces[index]);
        }
    }

	public e_Faces getFace(){
		return this.currentFace;
	}
}
