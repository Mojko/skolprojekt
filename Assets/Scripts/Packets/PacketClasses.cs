using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class DisconnectPacket : MessageBase{
    public string name = "";
}

public class SkillInfo : MessageBase 
{
	public int id;
	public string playerName;
	public int currentPoints;
	public int maxPoints;
}

public class StatInfo : MessageBase 
{
	public string playerName;
	public int value;
	public e_StatType statType;
}

public class ProjectTileInfo : MessageBase
{
    public string pathToObject;
    public string pathToEffect;
    public Vector3 spawnPosition;
    public Vector3 rotationInEuler;
}

public class InventoryInfo : MessageBase
{
    public NetworkInstanceId id;
    public int[] items;
    public string name = "";
}

public class MonsterInfo : MessageBase 
{
    public byte[] monster;
}

public class PlayerInfo : MessageBase
{
    public NetworkInstanceId id;
    public string name;
    public string characterName;
    public int level = 0;
    public int[] skillProperties;
}

public class LoginPacket : MessageBase
{
    public bool successfull = false;
    public string notSuccessfullReason = "";
    public string name = "";
    public string password = "";
}

public class moveItem : MessageBase
{
    public int[] item1 = new int[] { -1 };
    public int[] item2 = new int[] { -1 };
    public float[] position = new float[3];
    public string player;
}
public class PacketMessage : MessageBase
{
    public string message = "";
    public MessageTypes type;
    public string reciver;
    public string sender;
}
public class loadCharacters : MessageBase{
    public bool successfull = false;
    public string notSuccessfullReason = "";
    public string name = "";
    public string password = "";
    public int[] itemsEquip;
	public string[] colorScheme;
	public string[] names;
	public int[] stats;
    public int[] skillProperties;
    //public byte[] skillClassInBytes;
}
public class NPCInteractPacket : MessageBase
{
    public int state;
    public string sender;
    public int npcID;
    public NPCTalkType type;
    public string npcText;
}
public class OnPickCharacterPacket : MessageBase {
    public string characterName;
    public string userName; 
}