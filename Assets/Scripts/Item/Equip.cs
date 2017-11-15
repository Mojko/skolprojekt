using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Equip : Item
{
    public Equip(int keyID, int position, int inventoryType, params int[] stats) : base(keyID, position, inventoryType, stats)
    {
        this.setQuantity(1);
    }
}
