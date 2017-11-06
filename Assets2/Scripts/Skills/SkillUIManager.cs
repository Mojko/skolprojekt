using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUIManager : MonoBehaviour {
    private float xRoot = -75;
    private float yRoot = -15;
    private Player player;
    public GameObject actionBarPrefab;
    public GameObject parentForActionBarObjects;

    public List<Skill> skillsInActionBar = new List<Skill>();
    private Dictionary<int, KeyCode> keys = new Dictionary<int, KeyCode>();

    private float maxSpaceInAbilityBar = 4;
    

    public void init(Player player)
    {
        this.player = player;
        bindKey(KeyCode.Z, 0);
        bindKey(KeyCode.Y, 1);
        bindKey(KeyCode.Comma, 2);
        Debug.Log("wewewewew");
    }

    public void addNewSkillToActionBar(Skill skill)
    {
        if(skillsInActionBar.Count >= maxSpaceInAbilityBar) return;
        skillsInActionBar.Add(skill);
        GameObject actionBarObject = Instantiate(this.actionBarPrefab);
        actionBarObject.transform.SetParent(parentForActionBarObjects.transform);
        actionBarObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(xRoot+(50*(skillsInActionBar.Count-1)), yRoot, 0);
    }

    public Skill isPlayerTryingToActivateSkill()
    {
        for(int i=0;i<keys.Count;i++){
            Debug.Log("KEYS: " + keys[i]);
            if (Input.GetKey(keys[i])) {
                return skillsInActionBar[i];
            }
        }
        return null;
    }

    public void bindKey(KeyCode key, int skillBox)
    {
        keys.Add(skillBox, key);
    }
}
