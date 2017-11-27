using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Networking;
public enum inventoryTabs {
    EQUIPPED = -1, EQUIP = 0, USE, ETC, QUEST, MONEY
};
public enum ErrorID
{
    INVALID_ITEM = 0, HACK
};
public enum e_itemTypes
{
   EQUIP=-1, USE = 500, ETC = 1000, HATS = 1500, ACCESSORY = 2000, FACE = 2500, WEAPON = 3000, SHIELD = 3500, BODY = 4000, GLOVE = 4500, PANTS = 5000, BOOTS = 5500, NPC = 6000, QUESTS = 6500, MOBS, COIN = 9500
};
public enum EquipSlot {
    HAT, ACCESSORY, FACE, WEAPON, SHIELD, TOP, GLOVES, PANTS, BOOTS
};

public class ConnectGameObject {
	public void updateClients(NetworkInstanceId netId, Vector3 pos, Quaternion rot){
		RpcPositionToClients(netId, pos, rot);
	}
	public void updateServerAndClients(NetworkInstanceId netId, Vector3 pos, Quaternion rot){
		CmdPositionToServer(netId, pos, rot);
	}
	[Command]
	void CmdPositionToServer(NetworkInstanceId netId, Vector3 pos, Quaternion rot){
		GameObject o = NetworkServer.FindLocalObject(netId);
		o.transform.position = pos;
		o.transform.rotation = rot;
		RpcPositionToClients(netId, pos, rot);
	}

	[ClientRpc]
	void RpcPositionToClients(NetworkInstanceId netId, Vector3 pos, Quaternion rot){
		GameObject o = ClientScene.FindLocalObject(netId);
		o.transform.position = pos;
		o.transform.rotation = rot;
	}
}

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
    ITEMS
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
	public static UnityEngine.Object[] sprites = Resources.LoadAll("use");

	public static GameObject findInactiveChild(GameObject parent, string name){
		Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);
		foreach(Transform t in transforms){
			if(t.name.Equals(name)){
				return t.gameObject;
			}
		}
		return null;
	}
	public static Texture2D spriteToTexture(Sprite sprite){
		Texture2D generatedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
		Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
		generatedTexture.SetPixels(pixels);
		generatedTexture.Apply();
		return generatedTexture;
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
    public static GameObject[] getChildren(GameObject parent, params string[] children) {
        GameObject[] returnObject = new GameObject[children.Length];
        int foundChildren = 0;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (foundChildren >= returnObject.Length)
                return returnObject;
            for (int j = 0; j < children.Length; j++)
            {
                if (parent.transform.GetChild(i).name.Equals(children[j]))
                {
                    returnObject[j] = parent.transform.GetChild(i).gameObject;
                    foundChildren++;
                    continue;
                }
                if (parent.transform.GetChild(i).childCount > 0)
                {
                    GameObject child = getChild(parent.transform.GetChild(i).gameObject, children[j]);
                    if (child != null)
                        if (child.name.Equals(children[j]))
                        {
                            returnObject[j] = child.gameObject;
                            foundChildren++;
                        }
                }
            }
        }
        return returnObject;
    }
    public static Color hexColor(int hex)
    {
        int r = (hex >> 16) & 0xFF;
        int g = (hex >> 8) & 0xFF;
        int b = hex & 0xFF;
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
    public static bool isItemEquip(int itemID) {
        return (itemID.isItemType(e_itemTypes.HATS) || itemID.isItemType(e_itemTypes.PANTS) || itemID.isItemType(e_itemTypes.BODY) || itemID.isItemType(e_itemTypes.BOOTS) || itemID.isItemType(e_itemTypes.WEAPON) || itemID.isItemType(e_itemTypes.GLOVE) || itemID.isItemType(e_itemTypes.FACE) || itemID.isItemType(e_itemTypes.ACCESSORY));
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
		if (type == e_itemTypes.EQUIP) return isItemEquip(itemID);
		int ID = (int)(Mathf.Ceil((itemID + 1) / (Tools.ITEM_INTERVAL*1f)) * Tools.ITEM_INTERVAL);
		return ID == (int)type;
	}
    public static Sprite getSprite(this int itemID) {
        ItemVariables vars = ItemDataProvider.getInstance().getStats(itemID);
        return (Sprite)sprites[vars.getInt("imageIndex")];
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