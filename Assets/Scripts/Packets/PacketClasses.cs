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
public class playerEquipItem : MessageBase {
    public NetworkInstanceId netId;
    public byte[] item;
}
public class DropInfo : MessageBase {
	public NetworkInstanceId netId;
	public byte[] item;
}

public class OtherPlayerInfo : MessageBase {
	public byte[] equipment, color;
	public NetworkInstanceId id;
	public string characterName;
}

public class DamageInfo : MessageBase
{
    public NetworkInstanceId clientNetworkInstanceId;
    public NetworkInstanceId enemyNetworkInstanceId;
    public int damage;
    public e_DamageType damageType;
	public e_DamageTarget damageTarget;
	public Vector3 textPosition;
	public float damageMultiplier;
}

public class ItemInfo : MessageBase
{
    public NetworkInstanceId netId;
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

public class KillInfo : MessageBase {
	public int exp;
	public Vector3 rewardTextPosition;
}
public class EmptyInfo : MessageBase {
}
public class LevelUpInfo : MessageBase {
	public int expRequiredForNextLevel;
}

public class SkillCastInfo : MessageBase 
{
	public NetworkInstanceId netId;
	public NetworkInstanceId enemyNetId;
	public string pathToObject;
	public string pathToEffect;
	public Vector3 targetPosition;
	public Vector3 rotationInEuler;
	public string skillType;
	public float range;
	public Vector3 offset;
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
	public int exp;
	public Vector3 position;
	public Vector3 rotation;
	public NetworkInstanceId netId;
}

public class MonsterKill : MessageBase {
	public int exp;
	public Vector3 rewardTextPosition;
}

public class PlayerInfo : MessageBase
{
    public NetworkInstanceId id;
    public string name;
    public string characterName;
    public int[] skillProperties;
	public byte[] questClasses;
    public byte[] stats;
	public byte[] items;
	public byte[] equipment;
    public byte[] color;
	public int health;
	public byte[] otherPlayers;
	public int exp;
	public Vector3 position;
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
    public string playerName = "";
    public string password = "";
    public byte[] itemsEquip;
	public byte[] colorScheme;
	public string[] names;
	public byte[] stats;
    public int[] skillProperties;
    public int money;
    public NetworkInstanceId netID;
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
