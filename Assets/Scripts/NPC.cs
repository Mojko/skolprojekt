using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;
using System.Text;
/*
public class NPC : MonoBehaviour
{

    MainCamera mainCamera;

    bool isTalking = false;
    GameObject player;
    7Dialogue dialogue;
    //public GameObject prefabDialogueCanvas;
    public Canvas dialogueCanvas;
    private Canvas activeCanvas;
    Text dialogueText;
    public float textSpeed;
    public GameObject itemButton;
    public int id = 100000001;
    void Start()
    {
        mainCamera = Camera.main.GetComponent<MainCamera>();
        //dialogue = new Dialogue(this);
        //dialogue.startNewDialogue("|t|0.9|t|OMG|t|orgSpeed|t| Hello fellow stranger. I need some help, can you please collect five |I|1000|I| and ten |I|1002|I|", textSpeed, 0, this);
        //dialogue.startNewDialogue("|t|0.2|t|Reddit|t|orgSpeed|t| Hello fellow strawewe |I|1000|I| and ten |I|1002|I|", textSpeed, 0, this);

    }
    public int getID()
    {
        return id;
    }
    public void startDialogue(string dialogueText) {
        dialogue.startNewDialogue(dialogueText, textSpeed, 0, this);
        Debug.Log("added dialogue");
    }
    void Update()
    {
        if (isTalking && dialogue.getDialogueLength() != 0)
        {
            rotateTowards(mainCamera.player.transform.position);
            dialogue.runCurrentDialogue();
            dialogueText.text = dialogue.getString();
        }
    }
    public void stopTalking()
    {
        this.isTalking = false;
        dialogue.resetDialogue();
        Destroy(activeCanvas.gameObject);
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player") && !isTalking)
        {
            activeCanvas = Instantiate(dialogueCanvas, Vector3.zero, Quaternion.identity) as Canvas;
            dialogueText = activeCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            dialogueText.text = "";
            isTalking = true;
        }
    }

    void rotateTowards(Vector3 pos)
    {
        float x = (pos - this.transform.position).normalized.x;
        float y = 0;
        float z = (pos - this.transform.position).normalized.z;

        Quaternion rot = Quaternion.LookRotation(new Vector3(x, y, z));
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.2f);
    }
}
*/