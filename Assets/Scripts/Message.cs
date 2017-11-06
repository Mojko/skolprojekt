using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_dialogueType {
	OK,
	NEXT,
	YESNO
}

public class Message {

	public string fullstring = "";
	public int positionInString;
	public bool hasReadPage;

	public float timer;
	public float startTime;
	public int type;
	public string colorString = "";
    private Dialogue dialogue;
    public float orgSpeed;
    private int stringPos = 0;
    public Message(Dialogue dialogue,string fullstring, float delayBetweenCharacters=1f, int type=0, bool hasReadPage=false){
		this.fullstring = fullstring;
		this.timer = delayBetweenCharacters;
		this.startTime = delayBetweenCharacters;
		this.type = type;
        this.orgSpeed = delayBetweenCharacters;
		this.hasReadPage = hasReadPage;
        this.dialogue = dialogue;
	}
    //<<color>wew</color><color>wew2</color>

    //<<color>wew</color><color></color>
    public bool isFinished() {
        return (stringPos >= fullstring.Length);
    }
    public void resetMessage() {
        colorString = "";
        stringPos = 0;
        hasReadPage = false;
    }
    public string getFinishedString() {
        //Debug.Log("finished");
        colorString = "";
        for (int i = 0; i < fullstring.Length; i++) {
            if (fullstring.Length - i >= 3 && dialogue.checkForTags(i, fullstring[i] + "" + fullstring[i + 1] + "" + fullstring[i + 2], colorString))
            {
                colorString += dialogue.getFoundTagValue(i, fullstring);
                i += dialogue.getTagValueLength();
            }
            else
            {
                if (i >= fullstring.Length)
                    break;

                colorString += fullstring[i];
            }
            this.timer = startTime;
        }
        stringPos = fullstring.Length;
        return colorString;
    }

    public void run ()
	{
		if (timer <= 0) {
            if (stringPos >= fullstring.Length)
            {
                return;
            }

            if (fullstring.Length - stringPos >= 3 && dialogue.checkForTags(stringPos, fullstring[stringPos] + "" + fullstring[stringPos + 1] + "" + fullstring[stringPos + 2], colorString))
            {
                colorString += dialogue.getFoundTagValue(stringPos, fullstring);
                stringPos += dialogue.getTagValueLength();
            }
            else {
                colorString += fullstring[stringPos];
            }
			this.timer = startTime;
            stringPos++;
		} else {
			timer -= 1 * Time.deltaTime;
		}
	}
}
