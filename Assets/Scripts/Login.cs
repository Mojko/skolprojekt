using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
public class Login : NetworkBehaviour {

    public NetworkManager manager;
    public string name;
	public string characterName = "NoName";
    public NetworkClient client;
    public Text usernameT, passwordT;
	public MainCamera camera;
	public GameObject charSelect;
	public GameObject charCreate;
    public CreateCharacterUI createCharacterUI;
	public GameObject playerModel, signModel;
    private bool hasClicked = false;
    public GameObject world, loginWorld;
    public int[] skillProperties;
    public loadCharacters packet;
	void Update () {
		if (Input.GetKey (KeyCode.N)) {
			//Debug.Log(GetComponentInParent<Transform>().gameObject.name);
		}
	}
    public void Start()
    {
    }
    public string getName() {
        return this.name;
    }
    public string getCharacterName() {
        return this.characterName;
    }
    public void OnConnected(NetworkMessage msg)
    {
        Debug.Log("Connected to server");
        client.RegisterHandler(PacketTypes.LOGIN, loginPlayer);
        LoginPacket packet = new LoginPacket();
        packet.name = usernameT.text;
        packet.password = passwordT.text;
        Debug.Log("on login: " + packet.name);
        client.Send(PacketTypes.LOGIN, packet);
    }

    public void OnDisconnected(NetworkMessage msg)
    {
        Debug.Log("Disconnected from server");
    }

    public void OnError(NetworkMessage msg)
    {
        Debug.Log("Error connecting with code ");
    }
    public void onLogin() {
        if (!hasClicked)
        {
            hasClicked = true;
            //client.Disconnect();
            client = new NetworkClient();
            //client = manager.StartClient();
            client.RegisterHandler(MsgType.Connect, OnConnected);
            client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            client.Connect("localhost", 7777);
        }
    }
	public void createCharacters(loadCharacters characters){
		int length = characters.names.Length;
        List<List<Equip>> equips = (List<List<Equip>>)Tools.byteArrayToObject(characters.itemsEquip);
        List<string[]> colors = (List<string[]>)Tools.byteArrayToObject(characters.colorScheme);
        Debug.Log("Equips size:" + equips.Count);
        for (int i = 0; i < length; i++) {
            GameObject child = (GameObject)Instantiate (playerModel, Vector3.zero,Quaternion.identity);
            GameObject[] skin = Tools.getChildren(child.transform.GetChild(1).gameObject, "BodyModel", "HeadModel");
            GameObject[] eyes = Tools.getChildren(child.transform.GetChild(1).gameObject, "Eye_L_Model", "Eye_R_Model");
            setSkinColor(skin, colors[i][2]);
            setEyeColor(eyes, colors[i][1]);
            child.transform.SetParent (this.charSelect.transform.GetChild(i));
			child.transform.localPosition = Vector3.zero;
            this.charSelect.transform.GetChild(i).transform.localRotation = Quaternion.Euler(0,135,0);
            PickCharacter pickChar = this.charSelect.transform.GetChild(i).GetChild(0).gameObject.AddComponent<PickCharacter>();
            pickChar.name = characters.names[i];
            pickChar.setLogin(this);
            equipItems(equips[i],pickChar);
            this.gameObject.transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(1200, 0, 0);
            this.charSelect.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.GetComponent<TextMesh>().text = characters.names[i];
            pickChar.camera = camera.gameObject.GetComponent<Camera>();
		}
        for (int i = length; i < this.charSelect.transform.childCount; i++) {
            GameObject child = (GameObject)Instantiate(signModel, Vector3.zero, Quaternion.identity);
            child.transform.SetParent(this.charSelect.transform.GetChild(i));
            child.transform.localPosition = new Vector3(0,-3,0);
            child.transform.localRotation = Quaternion.Euler(0, -108, 0);
            child.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
            this.charSelect.transform.GetChild(i).transform.localRotation = Quaternion.Euler(0, 135, 0);
            CreateCharacter charCreate = this.charSelect.transform.GetChild(i).GetChild(0).gameObject.AddComponent<CreateCharacter>();
            charCreate.setCamera(this.camera);
            charCreate.setCharacter(this.charCreate, playerModel);
            charCreate.setUI(createCharacterUI);
        }
	}
    private void setSkinColor(GameObject[] skinModels, string color) {
        Color col;
        ColorUtility.TryParseHtmlString("#"+color, out col);
        for (int i = 0; i < skinModels.Length; i++) {
            Debug.Log("skinColor: " + col + " : " + color);
            skinModels[i].GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", col);
        }
    }
    private void setEyeColor(GameObject[] eyeModels, string color) {
        Color col;
        ColorUtility.TryParseHtmlString("#" + color, out col);
        for (int i = 0; i < eyeModels.Length; i++)
        {
            eyeModels[i].GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", col);
        }
    }
    private void equipItems(List<Equip> items, PickCharacter character) {

        GameObject[] playerEquipSlots = Tools.getChildren(character.gameObject, "hatStand", "weaponStand");
        GameObject[] clothes = Tools.getChildren(character.gameObject, "Shirt", "Pants");
        for (int i = 0; i < items.Count; i++) {
            if (items[i].getID().isItemType(e_itemTypes.HATS) || items[i].getID().isItemType(e_itemTypes.WEAPON))
            {
                Player.setEquipModel(items[i], playerEquipSlots);
            }
            else {
                Player.setClothes(items[i], clothes);
            }
        }
        //Player.setEquipModel(item, playerEquipSlots);

    }
    private void setColor(GameObject player,string[] colors) {
        setMeshColor(player.transform.GetChild(2).gameObject.GetComponent<SkinnedMeshRenderer>(), colors[0], colors[1]);
    }
    private void setMeshColor(SkinnedMeshRenderer meshRenderer, params string[] colors) {
        for (int i = 0; i < colors.Length; i++) {
            //Color color = new Color();
            //meshRenderer.materials[i].SetColor("_Color",colors[i]);
        }
    }
    public void loadWorld() {
        Destroy(this.loginWorld);
		this.world.SetActive(true);
    }

    public void onCharacterCreated(CreateCharacterButton[] values, string name) {
        loadCharacters packet = new loadCharacters();
        packet.name = name;
        Item[] equips = new Item[4];
        string[] color = new string[2];
        for (int i = 0; i < values.Length; i++) {
            if (values[i].isItem)
            {
                switch (values[i].itemType) {

                    case "Weapon":
                        equips[0] = new Item(values[i].itemID);
                    break;

                    case "Shirt":
                        equips[1] = new Item(values[i].itemID);
                    break;

                    case "Pants":
                        equips[2] = new Item(values[i].itemID);
                    break;

                    case "Shoes":
                        equips[3] = new Item(values[i].itemID);
                    break;
                }
            }
            else {
                if (values[i].colorType == "Skin") {
                    color[0] = ColorUtility.ToHtmlStringRGB(values[i].color);
                }
                else if (values[i].colorType == "Eye")
                {
                    color[1] = ColorUtility.ToHtmlStringRGB(values[i].color);
                }
            }
        }
        packet.itemsEquip = Tools.objectToByteArray(equips);
        packet.colorScheme = Tools.objectToByteArray(color);
        packet.playerName = this.name;
        client.Send(PacketTypes.CHARACTER_CREATE, packet);
    }

    public void loginPlayer(NetworkMessage msg) {
		loadCharacters packet = msg.ReadMessage<loadCharacters>();
        this.packet = packet;
        if (!packet.successfull) {
           	Debug.Log("not successfull: " + packet.notSuccessfullReason);
            client.Disconnect();
            return;
        }
		createCharacters (packet);
		camera.panTo (new Vector3(12,1.5f,33f),Quaternion.Euler(0,-50,0),2f,3f);
        this.skillProperties = packet.skillProperties;
        Debug.Log("successfull: " + packet.name);
        this.name = packet.name;
        
		//camera.panTo (new Vector3(-1,3,-1),Quaternion.Euler(0,55,0),0.2f,0.2f);
    }
}
