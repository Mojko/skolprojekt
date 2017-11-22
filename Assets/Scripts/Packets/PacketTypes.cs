using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketTypes{
    //# PLAYER
    public static readonly short DISCONNECT = 1000;
    public static readonly short LOAD_PLAYER = 1001;
    public static readonly short SAVE_PLAYER = 1002;
    public static readonly short LOGIN = 1003;
    public static readonly short NPC_INTERACT = 1004;
    public static readonly short PICK_CHAR = 1005;
    //# INVENTORY
	public static readonly short SAVE_INVENTORY = 1006;
	public static readonly short LOAD_INVENTORY = 1007;
    public static readonly short INVENTORY_MOVE_ITEM = 1008;
    public static readonly short INVENTORY_DROP_ITEM = 1009;
    public static readonly short INVENTORY_PICKUP_ITEM = 1010;
    public static readonly short ITEM_USE = 1011;
    public static readonly short ITEM_UNEQUIP = 1012;
    public static readonly short ITEM_EQUIP = 1013;
    //# CHAT
    public static readonly short SEND_MESSAGE = 1100;

	//#SKILLS
	public static readonly short VERIFY_SKILL = 1200;
	public static readonly short SAVE_SKILLS = 1201;
	public static readonly short LOAD_SKILLS = 1202;
	public static readonly short ERROR_SKILL = 1203;

	//#ENEMIES
	public static readonly short MONSTER_SPAWN = 1301;


	//#STAT ALLOCATOR
	public static readonly short PLAYER_BUFF = 1401;

    //SKILL
    public static readonly short PROJECTILE_CREATE = 1501;

    public static readonly short SPAWN_ITEM = 1601;
    public static readonly short NPC_ACESS_STATE = 1602;

    //QUESTS
    public static readonly short QUEST_START = 1701;
    public static readonly short QUEST_END = 1702;
	public static readonly short QUEST_UPDATE = 1703;
	public static readonly short QUEST_COMPLETE = 1704;
    
    public static readonly short DEAL_DAMAGE = 1024;

	//STANDBY
	public static readonly short STANDBY_PICKUP = 1800;

    //ERROR
    public static readonly short ERROR = 9999;

}
public enum MessageTypes
{
    CHAT, PRIVATE_MESSAGE, EVERYONE, SERVER
};
public enum NPCTalkType
{
    NEXT, OK, ACCEPT, PREV, YESNO
};
