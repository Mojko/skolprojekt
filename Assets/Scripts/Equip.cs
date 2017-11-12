using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip : Item
{
    private int positionInEquip;
    public Equip(int keyID, int position, int inventoryType, params int[] stats) : base(keyID, position, inventoryType, stats)
    {
        positionInEquip = Mathf.Abs(position + 1);
    }
    public int getPositionInEquip() {
        return positionInEquip;
    }
}
