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
	public GameObject playerModel;
    public int[] skillProperties;

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
        //client.Disconnect();
        client = new NetworkClient();
        //client = manager.StartClient();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        client.Connect("localhost", 7777);
    }
	public void createCharacters(loadCharacters characters){
		int length = characters.names.Length;
        for (int i = 0; i < this.charSelect.transform.childCount; i++) {
            if (i < length) {
				GameObject child = (GameObject)Instantiate (playerModel, Vector3.zero,Quaternion.identity);
				child.transform.SetParent (this.charSelect.transform.GetChild(i));
				child.transform.localPosition = Vector3.zero;
                this.charSelect.transform.GetChild(i).transform.localRotation = Quaternion.Euler(0,-90,0);
                PickCharacter pickChar = this.charSelect.transform.GetChild(i).GetChild(0).gameObject.AddComponent<PickCharacter>();
                pickChar.name = characters.names[i];
                pickChar.setLogin(this);
                this.gameObject.transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(1200, 0, 0);
                this.charSelect.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.GetComponent<TextMesh>().text = characters.names[i];
                pickChar.camera = camera.gameObject.GetComponent<Camera>();
            }
		}
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
    public void loginPlayer(NetworkMessage msg) {
		loadCharacters packet = msg.ReadMessage<loadCharacters>();
        if (!packet.successfull) {
           	Debug.Log("not successfull: " + packet.notSuccessfullReason);
            client.Disconnect();
            return;
        }
		createCharacters (packet);
		camera.panTo (new Vector3(0,0,1),Quaternion.Euler(0,90,0),3f,3f);
        this.skillProperties = packet.skillProperties;
        Debug.Log("successfull: " + packet.name);
        this.name = packet.name;
        
		//camera.panTo (new Vector3(-1,3,-1),Quaternion.Euler(0,55,0),0.2f,0.2f);
    }
}
