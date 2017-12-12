using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum e_Faces
{
    DEFAULT,
    ANGRY,
	ATTACK,
    TALKING
}

public class FaceManager : NetworkBehaviour {
	
    public Texture[] faces;
    public e_Faces[] faceTypes;
	private e_Faces currentFace;
	private AI ai;

	public void initilize(AI ai){
		this.ai = ai;
	}

    public void setFace(Renderer renderer, e_Faces face)
    {
        int index = (int)face;
		currentFace = face;
        if(faceTypes[index].Equals(face)){
            renderer.material.SetTexture("_MainTex", faces[index]);
        }
		if(isServer){
			ai.RpcSetFace(face);
		}
    }

	public e_Faces getFace(){
		return this.currentFace;
	}
}
