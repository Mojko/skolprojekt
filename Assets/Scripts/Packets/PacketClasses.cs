using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class DisconnectPacket : MessageBase{
    public string name = "";
    public byte[] questListInBytes;
    //public List<byte[]> questInBytes;
}

public class NetworkInstanceIdInfo : MessageBase {
	public NetworkInstanceId netId;
}

public class DropInfo : MessageBase {
	public NetworkInstanceId netId;
	public byte[] item;
}

public class DamageInfo : MessageBase
{
    public NetworkInstanceId clientNetworkInstanceId;
    public NetworkInstanceId enemyNetworkInstanceId;
    public int damage;
    public e_DamageType damageType;
}

public class ItemInfo : MessageBase
{
    public byte[] item;
    public byte[] oldItem;
    public byte[] itemVariables;
}

public class MobInfo : MessageBase
{
    public int mobId;
    public int amount;
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
	public NetworkInstanceId netId;
    public string pathToObject;
    public string pathToEffect;
    public Vector3 spawnPosition;
    public Vector3 rotationInEuler;
}

public class InventoryInfo : MessageBase
{
    public NetworkInstanceId id;
    public byte[] items;
    public byte[] equipment;
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
    public int[] skillProperties;
	public byte[] questClasses;
    public PlayerStats stats;
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
    public byte[] item1;
    public byte[] item2;
    public int[] position;
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
    public int money;
    //public byte[] skillClassInBytes;
}
public class NPCInteractPacket : MessageBase
{
    public int state;
    public string sender;
    public int npcID;
    public NPCTalkType type;
    public string npcText;

    public NetworkInstanceId playerInstanceId;

}
public class OnPickCharacterPacket : MessageBase {
    public string characterName;
    public string userName; 
}
public class ErrorMessage : MessageBase {
    public string message;
    public int errorID;
    public bool shouldKick;
}
public class QuestInfo : MessageBase
{
	public byte[] questClassInBytes;
}
