using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDataProvider {
    private static MonsterDataProvider provider;
    private ItemData mobs;
    //säker konstruktor som ser till att ingt annat objekt kan använda konstruktorn.
    protected MonsterDataProvider()
    {
        mobs = ItemDataProviderFactory.getItemProvider(new ItemDirectory(JsonManager.getPath(e_Paths.JSON_MONSTERS)));
    }
    //returnar nytt objekt om det är null annars returnar den provider variabeln.
    public static MonsterDataProvider getInstance()
    {
        if (provider == null)
            provider = new MonsterDataProvider();

        return provider;
    }
}
public class MobData {

}
