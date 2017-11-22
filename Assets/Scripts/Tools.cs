using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Networking;
using System;
public enum inventoryTabs {
    EQUIPPED = -1, EQUIP = 0, USE, ETC, QUEST, MONEY
};
public enum ErrorID
{
    INVALID_ITEM = 0, HACK
};
public enum e_itemTypes
{
    USE = 500, ETC = 1000, HATS = 1500, ACCESSORY = 2000, FACE = 2500, WEAPON = 3000, SHIELD = 3500, BODY = 4000, GLOVE = 4500, PANTS = 5000, BOOTS = 5500, NPC = 6000, QUESTS = 6500, MOBS
};
public enum EquipSlot {
    HAT, ACCESSORY, FACE, WEAPON, SHIELD, TOP, GLOVES, PANTS, BOOTS
};

public static class DefaultIds {
	/*
		0-500 = pots
		501-1000 = etc
		1001 - 1500 = hats
		1501 - 2000 = accessory
		2001 - 2500 = face
		2501 - 3000 = Weapon
		3001 - 3500 = Shield
		3501 - 4000 = Body
		4001 - 4500 = Glove
		4501 - 5000 = Pants
		5001 - 5500 = Boots
		5501 - 6000 = NPC
		6001 - 6500 = quests
		> 10000 = mobs
	*/
	public static int getNpcDefault(){
		return 5501;
	}
}

public enum e_Paths {
	JSON_QUESTS,
	JSON_SKILLTREE,
	JSON_MONSTERS,
    USE
}

public static class JsonManager {
	public static readonly string[] paths = {
		Application.persistentDataPath+"/Resources/Visuals/Quests.json",
		Application.persistentDataPath+"/Resources/Visuals/SkillTree.json",
		Application.persistentDataPath+"/Resources/Visuals/Monster.json",
        Application.persistentDataPath+"/Resources/Visuals/Items.I"
    };

	public static T readJson<T>(e_Paths path){
		Debug.Log("PATH: " + getPath(path));
		T json = JsonUtility.FromJson<T> (File.ReadAllText(getPath(path)));
		return json;
	}

	public static string getPath(e_Paths path){
		return paths[(int)path];
	}
}

public static class Tools
{
    public static readonly int ITEM_PROPERTY_SIZE = 15;
    public static readonly int ITEM_INTERVAL = 500;

	public static GameObject findInactiveChild(GameObject parent, string name){
        RectTransform[] transforms = parent.GetComponentsInChildren<RectTransform>(true);
		foreach(RectTransform t in transforms){
            Debug.Log("names!!!!: " + t.name);
			if(t.name.Equals(name)){
				return t.gameObject;
			}
		}
		return null;
	}
    public static T CastData<T>(object input)
    {
        return (T)input;
    }

    public static T ConvertData<T>(object input)
    {
        return (T)Convert.ChangeType(input, typeof(T));
    }
    public static GameObject loadObjectFromResources(e_Objects obj)
    {
        return (GameObject)Resources.Load(ResourceStructure.getPathForObject(obj));
    }

	public static byte[] objectToByteArray(object obj){
		if (obj == null)
			return null;

		BinaryFormatter bf = new BinaryFormatter ();
        
		using (MemoryStream ms = new MemoryStream ()) {
			bf.Serialize (ms,obj);
			return ms.ToArray();
		}
	}

	public static byte[] objectArrayToByteArray(object[] obj){
		if (obj == null)
			return null;

		BinaryFormatter bf = new BinaryFormatter ();

		using (MemoryStream ms = new MemoryStream ()) {
			bf.Serialize (ms,obj);
			return ms.ToArray();
		}
	}

	public static object byteArrayToObject(byte[] byteArray){
		MemoryStream memStream = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		memStream.Write (byteArray, 0, byteArray.Length);
		memStream.Seek (0, SeekOrigin.Begin);
		object obj = (object)bf.Deserialize (memStream);
		return obj;
	}
	public static object[] byteArrayToObjectArray(byte[] byteArray){
		MemoryStream memStream = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		memStream.Write (byteArray, 0, byteArray.Length);
		memStream.Seek (0, SeekOrigin.Begin);
		object[] obj = (object[])bf.Deserialize (memStream);
		return obj;
	}

    public static GameObject[] getChildrenByTag(GameObject parent, string tag)
    {
        List<GameObject> objects = new List<GameObject>();
        for(int i = 0; i < parent.transform.childCount; i++) {
            if (parent.transform.GetChild(i).CompareTag(tag)) {
                objects.Add(parent.transform.GetChild(i).gameObject);
            }
        }
        return objects.ToArray();
    }

    public static GameObject getChild(GameObject parent, string name)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).name.Equals(name))
                return parent.transform.GetChild(i).gameObject;
            if (parent.transform.GetChild(i).childCount > 0)
            {
                GameObject child = getChild(parent.transform.GetChild(i).gameObject, name);
                if (child == null)
                    continue;
                if (child.name.Equals(name))
                    return child.gameObject;
            }
        }
        return null;
    }
    public static Color hexColor(int hex)
    {
        int r = (hex >> 16) & 0xFF;
        int g = (hex >> 8) & 0xFF;
        int b = hex & 0xFF;
        Debug.Log(r + " + " + g + " + " + b);
        return new Color(r / 255f, g / 255f, b / 255f, 1);
    }
    public static Transform[] getAllChildren(this Transform parent)
    {
        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = parent.GetChild(i);
        }
        return children;
    }
    public static GameObject[] transformsToObject(this Transform[] transforms) {
        GameObject[] children = new GameObject[transforms.Length];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = transforms[i].gameObject;
        }
        return children;
    }
    public static void Lerp(this float item, float b, float time) {
        item = Mathf.Lerp(item, b, time);
    }


    public static PlayerServer getPlayer(this NetworkMessage msg)
    {
        return Server.playerObjects[msg.conn.connectionId];
    }
    public static bool isItemType(this int itemID, e_itemTypes type) {
        return (Mathf.Ceil((itemID + 1) / 500f) * 500 == (int)type);
    }
    public static ItemDataAll getChild(this ItemDataAll data, int itemID) {
        return null;
    }
}

namespace GlobalTools
{
    class ToolsGlobal
    {
		public Quaternion rotateTowards(Transform transform, Vector3 targetPosition, float damping) {
			Quaternion r = Quaternion.LookRotation(targetPosition - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, r, damping * Time.deltaTime);
			return r;
	    }
		public Quaternion rotateTowards(Transform transform, Vector3 targetPosition) {
			Quaternion r = Quaternion.LookRotation(targetPosition - transform.position);
			return r;
	    }
    }
}