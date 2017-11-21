using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
public class ItemDataProvider {
    private static ItemDataProvider provider;
    private ItemData items;

    protected ItemDataProvider() {
        items = ItemDataProviderFactory.getItemProvider(new ItemDirectory(JsonManager.getPath(e_Paths.USE)));
        Debug.Log("datapprovider: " + items);
    }
    public static ItemDataProvider getInstance() {
        if (provider == null)
            provider = new ItemDataProvider();

        return provider;
    }
    public ItemVariables getStats(int itemID) {
        Debug.Log("datapprovider: " + items);
        return items.getItemStats(itemID);
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
public class ItemDataProviderFactory{
    public static ItemData getItemProvider(ItemDirectory directory) {
        if (directory.isItemDirectory())
            return new ItemData(directory);

        return null;
    }
}
public class ItemDirectory {
    private string path;
    private int subFileSize;
    public List<ItemDirectory> subDirectories = new List<ItemDirectory>();
    public List<FileInfo> files = new List<FileInfo>();
    private DirectoryInfo directoryInfo;
    public ItemDirectory(string path) {
        constructor(path);
    }
    public ItemDirectory(DirectoryInfo directory) {
        constructor(directory.FullName);
    }

    private void constructor(string path)
    {
        directoryInfo = new DirectoryInfo(path);
        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        for (int i = 0; i < dirs.Length; i++)
        {
            subDirectories.Add(new ItemDirectory(dirs[i]));
        }
        FileInfo[] files = directoryInfo.GetFiles("*.json");
        for (int i = 0; i < files.Length; i++)
        {
            this.files.Add(files[i]);
        }
        this.path = path;
    }
    public FileInfo getFileContainingString(int itemID) {
        int normID = (int)Mathf.Ceil((itemID + 1) / 500f) * 500;
        foreach (FileInfo file in files)
        {
            if (file.Name.Contains(normID + ""))
            {
                Debug.Log("file found!!! " + file.Name);
                return file;
            }
        }
        foreach (ItemDirectory directory in subDirectories) {
            return directory.getFileContainingString(normID);
        }
        return null;
    }
    public string getPath() {
        return this.path;
    }
    public bool isItemDirectory() {
        return (path.ToLower().EndsWith(".i"));
    }
}
public class ItemData{
    ItemDirectory directory;
    private Dictionary<int, ItemVariables> itemValues = new Dictionary<int, ItemVariables>();
    public ItemData(ItemDirectory directory) {
        this.directory = directory;
    }
    public ItemVariables getItemStats(int itemID) {
        if (itemValues.ContainsKey(itemID)) {
            return itemValues[itemID];
        }
        FileInfo file = this.directory.getFileContainingString(itemID);
        StreamReader reader = file.OpenText();
        string fileContents = "";
        string line = "";
        while ((line = reader.ReadLine()) != null) {
            fileContents += line + Environment.NewLine;
        }
        ItemDataAll ItemDataAll = itemDataConverter(itemID, fileContents);
        ItemVariables variables = new ItemVariables();
        ItemDataPots pots = (ItemDataPots)ItemDataAll;
        variables.setVariables(ItemDataAll.getVariables());
        itemValues.Add(itemID, variables);
        return variables;
    }
    private ItemDataAll itemDataConverter(int itemID, string file) {
        Debug.Log(itemID);
        if (itemID.isItemType(e_itemTypes.USE)) {
            ItemDataPots data = JsonUtility.FromJson<ItemDataPots>(file);
            data.parentItems = data.items;
            data = (ItemDataPots)getItemFromParent(data,itemID);
            data.setVariables();
            return data;
        }
        if (itemID.isItemType(e_itemTypes.HATS) || itemID.isItemType(e_itemTypes.PANTS) || itemID.isItemType(e_itemTypes.BODY) || itemID.isItemType(e_itemTypes.BOOTS) || itemID.isItemType(e_itemTypes.WEAPON) || itemID.isItemType(e_itemTypes.GLOVE) || itemID.isItemType(e_itemTypes.FACE) || itemID.isItemType(e_itemTypes.ACCESSORY))
        {
            return JsonUtility.FromJson<ItemDataEquips>(file);
        }
        return null;
    }
    private ItemDataAll getItemFromParent(ItemDataAll data, int itemID) {
        return data.parentItems[itemID - data.startIndex];
    }
}
[Serializable]
public class ItemVariables {
    public Dictionary<string, object> variables = new Dictionary<string,object>();
    public void addVariable(string variableName, object value) {
        variables.Add(variableName, value);
    }
    public int getInt(string varName) {
        return (int)variables[varName];
    }
    public string getString(string varName)
    {
        return (string)variables[varName];
    }
    public float getFloat(string varName)
    {
        return (float)variables[varName];
    }
    public object getObject(string varName)
    {
        return variables[varName];
    }
    public void setVariables(Dictionary<string, object> variables) {
        this.variables = variables;
    }
}
[Serializable]
public class ItemDataAll {
    public int startIndex;
    public ItemDataAll[] parentItems;
    public Dictionary<string, object> variables = new Dictionary<string, object>();
    public ItemDataAll[] getData()
    {
        return parentItems;
    }
    public void setVariable(string name, object value) {
        variables.Add(name, value);
    }
    public Dictionary<string, object> getVariables() {
        return variables;
    }
    public void setVariables()
    {
        foreach (FieldInfo field in this.GetType().GetFields())
        {
            this.variables.Add(field.Name, field.GetValue(this));
        }
    }
    public void setVariables(Dictionary<string, object> variables)
    {
        this.variables = variables;
    }
}
[Serializable]
public class ItemDataPots : ItemDataAll
{
    public ItemDataPots[] items;
    public int id;
    public string name;
    public int health;
    public int damage;
    public int mana;
    public int imageIndex;
}
[Serializable]
public class ItemDataEquips : ItemDataAll
{
    public int id;
    public string name;
    public int health;
    public int mana;
    public int imageIndex;
}
[Serializable]
public class ItemDataScrolls : ItemDataAll
{
    public int id;
    public string name;
    public int health;
    public int mana;
    public int imageIndex;
}
[Serializable]
public class ItemDataEtc : ItemDataAll
{
    public int id;
    public string name;
    public int health;
    public int mana;
    public int imageIndex;
}
