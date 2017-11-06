using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class Spawner : NetworkBehaviour {

	public float radius;
    public Vector3 vector3Radius;
	public int maxEnemies;
	public int monsterId;
    public int totalEnemiesInArea;
    public Timer timer = new Timer(5, true);
    private Monster monster;

	void Start() 
    {
        if(!isServer) return;
        radius = vector3Radius.x;
        this.monster = Server.getMonsterFromJson(monsterId);

	}
    private void Update()
    {
        if(!isServer) return;
        timer.update();
        if (timer.isFinished() && totalEnemiesInArea < maxEnemies) {
            Server.spawnMonster(monsterId, randomVector3InRadius());
            totalEnemiesInArea++;
        }
    }
    private Vector3 randomVector3InRadius()
    {
        Vector3 pos = this.transform.position;
        return new Vector3(pos.x + Random.Range(-radius, radius), pos.y, pos.z + Random.Range(-radius, radius));
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,1,0,0.3f);
        Gizmos.DrawCube(this.transform.position, vector3Radius);
    }

}
