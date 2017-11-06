using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class mutliClick : MonoBehaviour, IPointerDownHandler
{
    public int clicks;
    public void OnPointerDown(PointerEventData eventData)
    {
        clicks = eventData.clickCount;
    }
    public bool getClicks(int amt) {
        if (clicks > amt) {
            clicks = 0;
            return true;
        }
        return false;
    }
}