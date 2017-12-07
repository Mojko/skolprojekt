using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue {
	
	public delegate void deleg(int position, bool hasEnded, string value);
	public deleg method;
	List<int[]> pos = new List<int[]>();
	public int page = 0;
	List<Message> queuedDialogues = new List<Message>();
	string colorString = "";
	string fullString = "";
    private string tagValue = "";
    private int tagValueLength = 0;
    int latestPosition = 0;
    Dictionary<string, deleg> checks = new Dictionary<string, deleg>();
    //"<colerd>wew</col><col></col>"
	public NPCMain npc;
    private string latestCommandSeen = "";
    public Dialogue(NPCMain npc) {

        check("|i|", getItem);
        check("|I|", getItems);
        check("|t|", getTime);


        this.npc = npc;
    }
    public int getDialogueLength() {
        return queuedDialogues.Count;
    }
    public void startNewDialogue (string fullstring, float delayBetweenCharacters, int type, NPCMain npc)
	{
		queuedDialogues.Add (new Message(this, fullstring, delayBetweenCharacters, type, false));
	}

    public string getFoundTagValue(int position,string fullMessage) {
        Debug.Log("found tag");
        for (int i = position + 3; i < fullMessage.Length; i++) {
            if (startChecking(i, fullMessage[i] + "" + fullMessage[i + 1] + "" + fullMessage[i + 2],fullMessage.Substring(position+3, i - position - 3))) {
                tagValueLength = i - position + 3;
                return tagValue;
            }
        }
        return "";
    }
    public int getTagValueLength() {
        int temp = tagValueLength;
        tagValue = "";
        tagValueLength = 0;
        latestCommandSeen = "";
        latestPosition = 0;
        return temp-1;
    }
    public bool checkForTags(int position, string message,string fullString) {
        return startChecking(position,message,fullString);
    }

	public Message getDialogue(){
		return queuedDialogues[page];
	}

	void forceFinishDialogue(){
        Debug.Log("finish");
		queuedDialogues [page].colorString = queuedDialogues [page].getFinishedString();
	}
	public void check (string s, deleg method) {
		checks.Add(s,method);
	}

	public void getItem (int position, bool hasEnded,string value) {
        string newValue = "<color=\"#6CB95DFF\">";
        if (hasEnded) {
            int p = int.Parse(value);
            newValue = ItemDataProvider.getInstance().getStats(p).getString("name") + "</color>";
        }
        tagValue += newValue;
	}
    //förvandlar |i| till color.
    public void getItems(int position, bool hasEnded, string value)
    {
        string newValue = "<color=\"#6CB95DFF\">";
        //kollar om den stötter på en stängd i.
        if (hasEnded)
        {
            int p = int.Parse(value);
            //hämtar item namnet beroende på vilket id det är mellan tagsen.
            newValue = ItemDataProvider.getInstance().getStats(p).getString("name") + "'s</color>";
        }
        tagValue += newValue;
    }
    //körs när den stöter på |t|.
    public void getTime(int position, bool hasEnded, string value)
    {
        if (hasEnded) {
            //om den innerhåller orgSpeed ska den gå tillbaka till normal fart. annars ska den öka beroende på vad som stog mellan.
            if (value.Equals("orgSpeed"))
            {
                queuedDialogues[page].startTime = queuedDialogues[page].orgSpeed;
            }
            else {
                queuedDialogues[page].startTime = float.Parse(value);
            }
        }
    }
    //kollar om denn stringen i man skickar in finns i checks directoryn. och om den finns ska den hämta metoden som var i directryn och kalla den.
	private bool startChecking (int position, string i,string value)
	{
		if (checks.ContainsKey (i)) {
			if (latestCommandSeen != i) {
				latestCommandSeen = i;
				latestPosition = position;
				checks[i](position,false,"");
			} else {
				latestCommandSeen = "";
				checks[i](position,true,value);
			}
            return true;
		}
        return false;
	}
	public void runCurrentDialogue () {
        //kör den nuvarande dialogen som är aktiv.
		queuedDialogues [page].run ();
        //om man trycker ned höger pil ska den antigen gå till nästa om dialogen är klar annars ska man tvinga klart dialogen så all text syns.
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			if (isDialogueFinished ()) {
                //används för om man vill gå tillbaka en dialog så behöver man inte se text animationen igen utan hela texten syn.
                queuedDialogues[page].hasReadPage = true;
				gotoNextDialogue ();
			} else {
                forceFinishDialogue();
			}
		}
        //om man har läst dialogen ska den tvingas bli klar.
		if (queuedDialogues [page].hasReadPage) {
			forceFinishDialogue();
		}
        //om man trycker på left dialogue ska den gå tillbaka en.
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			gotoPreviousDialogue();
		}
	}

	public void gotoNextDialogue () {
        //kollar så att man inte är på den sista dialogen. annars ska man sluta prata med NPCn.
        if (page < queuedDialogues.Count - 1)
        {
            //ökar en och resetar alla värden som används i en dialog.
            page++;
            tagValue = "";
            tagValueLength = 0;
            latestCommandSeen = "";
            latestPosition = 0;
        }
        else {
            exitDialogue();
        }
	}
    //kollar om dialogen är klar.
	public bool isDialogueFinished () {
        return queuedDialogues[page].isFinished();
	}

	public void gotoPreviousDialogue(){
		if (page > 0) {
			removeContent(page);
			page--;
            //hämatar den fulländade stringen istället för att visa karaktär för karaktär.
            queuedDialogues[page].getFinishedString();
		}
	}
	
	public string getString(){
		return queuedDialogues[page].colorString;
	}

	public void closeAllDialogues(){
		queuedDialogues.RemoveRange(0, queuedDialogues.Count);
	}
    public void resetDialogue() {
        this.page = 0;
        for (int i = 0; i < queuedDialogues.Count; i++)
        {
            queuedDialogues[i].resetMessage();
        }
        tagValue = "";
        tagValueLength = 0;
        latestCommandSeen = "";
        latestPosition = 0;
    }
	public void removeContent (int page) {
		queuedDialogues[page].colorString = string.Empty;
		queuedDialogues[page].timer = 0;
		queuedDialogues[page].positionInString = 0;
	}
    public void exitDialogue() {
        npc.stopTalking();
    }
}
