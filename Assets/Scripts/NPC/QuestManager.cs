using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{

    Player player;
    public Quest(Player player)
    {
        this.player = player;
    }

    public bool isCompleted()
    {
        return true;
    }
    public void startQuest()
    {
        player.getNetwork()
    }
}

public class QuestManager : MonoBehaviour {

}
