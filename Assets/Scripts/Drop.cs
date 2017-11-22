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

    Renderer renderer = null;

    private string nameOfDrop;
	public void initilize(Item item, PlayerServer playerServer){
		this.name = item.getName();
		this.playerServer = playerServer;
		this.item = item;
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
        move = true;
        dirX = Random.Range(0.5f,2);
        randomDir = chooseRandomDirection();
	}

    void Update()
    {
        if(move){
            dirX = Mathf.Lerp(dirX, 0, 0.25f * Time.deltaTime);
            //dirX -= 0.9f * Time.deltaTime;
            dirY -= 5f * Time.deltaTime;
            this.transform.position += (transform.up * 4 * Time.deltaTime) * -dirY;
            this.transform.position += (randomDir * dirX * Time.deltaTime);
        }
        RaycastHit hit;
        if(Physics.Raycast(this.transform.position, Vector3.down ,out hit, 1) && dirY <= 0){
            if(hit.transform.CompareTag("Ground")){
                move = false;
                this.GetComponent<FloatingEffect>().enabled = true;
            }
        }
    }


	void OnTriggerStay(Collider col) {
        //renderer.material.SetFloat("_Flash", 0.25f);
		if(!isServer) return;
		if (col.gameObject.CompareTag("Player") && Input.GetKey(KeyCode.B)) {
			/*Player player = col.gameObject.GetComponent<Player>();
			player.getNetwork().sendItem(PacketTypes.INVENTORY_PICKUP_ITEM, item);
            NetworkServer.Destroy(this.gameObject);*/
		}

	}
	void OnTriggerEnter(Collider col){
		if(!isServer) return;
		if(col.gameObject.CompareTag("Player")){
			ItemInfo itemInfo = new ItemInfo();
			itemInfo.item = Tools.objectToByteArray(this.item);
			NetworkServer.SendToClient(this.playerServer.connectionID, PacketTypes.STANDBY_PICKUP, itemInfo);
			Debug.Log("Sent!");
		}
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
