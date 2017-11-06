using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour {

    public List<float> timerQueue = new List<float>();
    public List<e_Objects> objectsQueue = new List<e_Objects>();

    void Update() {
        for(int i = 0; i < objectsQueue.Count; i++) {
            timerQueue[i] -= 1 * Time.deltaTime;
            if(getTime(i) <= 0) {
                Server.spawnObject(objectsQueue[i]);
                objectsQueue.RemoveAt(i);
                timerQueue.RemoveAt(i);
                break;
            }
        }
    }

    float getTime(int index) {
        return timerQueue[index];
    }

    public void startRespawnTimer(e_Objects o, float time) {
        Debug.Log("Respawning enemy...");
        objectsQueue.Add(o);
        timerQueue.Add(time); 
    }
}
