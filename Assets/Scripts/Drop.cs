using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Drop : NetworkBehaviour {
	private Item item;
    private GameObject model;
    private bool move;
    private float dirX;
    private float dirY = 4;
    private Vector3 randomDir;
	private PlayerServer playerServer;
	private NetworkIdentity netIdentity;
	private Player player;
	private bool isPickedUp = false;

    Renderer renderer = null;

    private string nameOfDrop;
	public void initilize(Item item){
		this.name = item.getName();
		this.item = item;
		Debug.Log("initilized drop: " + item.getID() + " | Server: " + isServer + " | Client: " + isClient);
	}

	public Item getItem(){
		return this.item;
	}

    float choose(float val1, float val2)
    {
        Random.seed = System.DateTime.Now.Millisecond;
        int r = Random.Range(0,4);
        if(r == 0) return val1; else return val2;
        return 0;
    }

	void Start(){
		/*this.transform.rotation = Quaternion.Euler (dropAbleObjects [0].transform.rotation.x, 0, dropAbleObjects [0].transform.rotation.z);
		this.transform.localScale = dropAbleObjects[0].transform.lossyScale;
		this.renderer = this.GetComponent<Renderer> ();*/
        this.renderer = this.GetComponent<Renderer>();
		this.netIdentity = GetComponent<NetworkIdentity>();
        move = true;
        dirX = Random.Range(0.5f,2);
        randomDir = chooseRandomDirection();
	}

    void Update()
    {
        /*
		if(!isServer) return;
		RpcPositionToClients(this.transform.position);
        if(move){
            dirX = Mathf.Lerp(dirX, 0, 0.25f * Time.deltaTime);
            //dirX -= 0.9f * Time.deltaTime;
            dirY -= 5f * Time.deltaTime;
            this.transform.position += new Vector3(0, 4 * Time.deltaTime * dirY,0);
            this.transform.position += (randomDir * dirX * Time.deltaTime);
        }
        RaycastHit hit;
        Debug.DrawRay(this.transform.position, Vector3.down * 5, Color.blue);
        if(Physics.Raycast(this.transform.position, Vector3.down ,out hit, 5)){
            if(hit.transform.CompareTag("Ground")){
                Debug.Log(this.transform.name);
                move = false;
				RpcActivateFloatingEffect();
            }
        }
        */
		//this.connectGameObject.updateServerAndClients(netIdentity.netId, this.transform.position, this.transform.rotation);
    }

	[ClientRpc]
	void RpcPositionToClients(Vector3 pos){
		transform.position = pos;
	}
	[ClientRpc]
	void RpcActivateFloatingEffect(){
		this.GetComponent<FloatingEffect>().enabled = true;
	}


	void OnTriggerStay(Collider col) {
		if(col.gameObject.CompareTag("Player")){
			if(isClient){
				//this.player.canPickup(this.gameObject);
				if(Input.GetKey(KeyCode.B) && !isPickedUp){
                    Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: " + this.item);
					Player p = col.GetComponent<Player>();
					p.getNetwork().sendItem(PacketTypes.PICKUP,this.item);
					isPickedUp = true;
					p.CmdDestroyObject(this.netIdentity);
					//p.getNetwork().destroyGameObject(this.GetComponent<NetworkIdentity>().netId);
				}
			}

		}

	}
	void OnTriggerEnter(Collider col){
		//this.player = col.GetComponent<Player>();
	}

    void OnTriggerExit(Collider col){
		
        //renderer.material.SetFloat("_Flash", 1);
        //this.renderer.material.color = Color.gray;	
	}
    Vector3 chooseRandomDirection()
    {
        return new Vector3(Random.Range(-1,1), 0, Random.Range(-1,1));
    }
}
