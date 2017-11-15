using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobHolder : MonoBehaviour {
    public Dictionary<int, MobManager> mobs = new Dictionary<int, MobManager>();

    public int addNewMob(MobManager mob)
    {
        mobs.Add(mobs.Keys.Count, mob);
        return mobs.Keys.Count;
    }
}
