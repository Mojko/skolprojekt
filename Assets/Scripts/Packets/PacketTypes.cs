﻿using System.Collections;
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

    //# CHAT
    public static readonly short SEND_MESSAGE = 1011;

	//#SKILLS
	public static readonly short VERIFY_SKILL = 1012;
	public static readonly short SAVE_SKILLS = 1013;
	public static readonly short LOAD_SKILLS = 1014;
	public static readonly short ERROR_SKILL = 1015;

	//#ENEMIES
	public static readonly short MONSTER_SPAWN = 1016;


	//#STAT ALLOCATOR
	public static readonly short PLAYER_BUFF = 1017;

    //SKILL
    public static readonly short PROJECTILE_CREATE = 1018;

    public static readonly short SPAWN_ITEM = 1019;

}
public enum MessageTypes
{
    CHAT, PRIVATE_MESSAGE, EVERYONE, SERVER
};
public enum NPCTalkType
{
    NEXT, OK, ACCEPT, PREV, YESNO
};
