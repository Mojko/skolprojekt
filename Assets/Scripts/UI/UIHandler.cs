using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour {
    protected bool isActive;
    public GameObject mover;
    bool setMouseValue = false;
    // Use this for initialization
    private Vector2 mouseDown = new Vector2(0,0);
    private int clicks = 0;
    private float lastClick = 0;
    private bool resizeActive = false;
    UIMouseHandler mouseHandler;
    public void Start() {
        initUI();
    }
    protected void initUI() {
        if (this.gameObject.GetComponent<Outline>() == null)
            this.gameObject.AddComponent<Outline>();

            if (this.gameObject.GetComponent<Image>() == null)
            this.gameObject.AddComponent<Image>();

            mouseHandler = this.transform.parent.GetComponent<UIMouseHandler>();
			if(mouseHandler == null) mouseHandler = this.transform.root.GetComponent<UIMouseHandler>();
            mouseHandler.addElement(this);
            this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(0, 0);
            this.gameObject.GetComponent<Outline>().effectColor = new Color(0.207f, 0.376f, 0.184f, 1);
            this.gameObject.GetComponent<Outline>().useGraphicAlpha = false; 
    }

    public void removeThisFromParent()
    {
        mouseHandler.removeElement(this);
    }

    public bool isClickingInsideUI()
    {
        return (RectTransformUtility.RectangleContainsScreenPoint(this.gameObject.GetComponent<RectTransform>(), Input.mousePosition, null));
    }
    public bool hasDoubleClicked() {
        if (Time.time - lastClick < 0.2f) {
            return true;
        }
        lastClick = Time.time;
        return false;
    }

	public bool canFocus(){
		if (!resizeActive && this.gameObject.activeSelf && Input.GetMouseButtonDown(0))
		{
			return true;
		}
		return false;
	}

    protected void setFocused()
    {
        //om man har klickat på ui elementet.
		if (canFocus())
        {
            if (isClickingInsideUI() && mouseHandler.canClick(this))
            {
                this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(4, -4);
                this.gameObject.transform.SetAsLastSibling();
            }
            else
            {
                this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(0, 0);
            }
        }
        //om man dubbel klickar på ui elementet.
        if (this.gameObject.activeSelf && Input.GetMouseButtonDown(0) && isClickingInsideUI())
        {
            if (hasDoubleClicked() && mouseHandler.canClick(this))
            {
                resizeActive = !resizeActive;
                if (resizeActive)
                {
                    this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(10, -10);
                    this.gameObject.GetComponent<Outline>().effectColor = new Color(0.423f, 0.725f, 0.364f, 1);
                }
                else
                {
                    this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(4, -4);
                    this.gameObject.GetComponent<Outline>().effectColor = new Color(0.207f, 0.376f, 0.184f, 1);
                }
            }
        }
        //om man dubbel klickar utanför ui elementet så ska resize canvas stängas av.
        else if (this.gameObject.activeSelf && Input.GetMouseButtonDown(0) && !isClickingInsideUI() && resizeActive && hasDoubleClicked())
        {
            this.gameObject.GetComponent<Outline>().effectDistance = new Vector2(0,0);
            this.gameObject.GetComponent<Outline>().effectColor = new Color(0.207f, 0.376f, 0.184f, 1);
            resizeActive = false;
        }
    }
    public bool isMouseOver(GameObject obj) {
        return (RectTransformUtility.RectangleContainsScreenPoint(obj.GetComponent<RectTransform>(), Input.mousePosition, null));
    }
    protected void onMove() {
        if (Input.GetMouseButtonDown(0) && isMouseOver(mover) && mouseHandler.canClick(this) || setMouseValue) {
            RectTransform rect = this.GetComponent<RectTransform>();
            if (!setMouseValue)
            {
                setMouseValue = true;
                mouseDown = new Vector2(Input.mousePosition.x - rect.position.x, Input.mousePosition.y - rect.position.y);
            }
            rect.position = new Vector3(Input.mousePosition.x - mouseDown.x, Input.mousePosition.y - mouseDown.y,0);
        }
        if (Input.GetMouseButtonUp(0)) {
            setMouseValue = false;
        }
    }
    public void Update() {
        onMove();
        setFocused();
    }
    public bool canClose() { return true; }
    public void Enable(bool condition)
    {
        if (!canClose()) return;
        isActive = condition;
        if (isActive)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
}
