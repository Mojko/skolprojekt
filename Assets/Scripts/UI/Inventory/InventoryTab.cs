using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTab : MonoBehaviour
{
    public GameObject inventoryItem;
    public InventoryUIHandler handler;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void onClick()
    {
        inventoryItem.transform.SetAsLastSibling();
        Debug.Log("woah dude: " + inventoryItem.name);
        handler.hasClickedTab(this);
    }
}
