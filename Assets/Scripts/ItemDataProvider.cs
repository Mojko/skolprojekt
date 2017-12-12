using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;

public class ItemDataProvider {
    private static ItemDataProvider provider;
    private ItemData items;
    private ItemData mobs;
    //säker konstruktor som ser till att ingt annat objekt kan använda konstruktorn.
    protected ItemDataProvider() {
		items = ItemDataProviderFactory.getItemProvider(new ItemDirectory(JsonManager.getPath(e_Paths.ITEMS)));
        //mobs = ItemDataProviderFactory.getItemProvider(new ItemDirectory(JsonManager.getPath(e_Paths.JSON_MONSTERS)));
        Debug.Log("datapprovider: " + items);
    }
    //returnar nytt objekt om det är null annars returnar den provider variabeln.
    public static ItemDataProvider getInstance() {
        if (provider == null)
            provider = new ItemDataProvider();

        return provider;
    }
    //hämr stats för ett specifikt item
    public ItemVariables getStats(int itemID) {
        return items.getItemStats(itemID);
    }
    public ItemVariables getMob(int mobID) {
        return null;
    }
}

public class ItemDataProviderFactory{
    //kollar om directory är en fil och om den är så skapar den en ny Data med det directoryn.
    public static ItemData getItemProvider(ItemDirectory directory) {
        if (directory.isItemDirectory())
        {
            return new ItemData(directory);
        }
        return null;
    }
    public static MobData getMobProvider() {
        return new MobData();
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
    //konstruktorn.
    private void constructor(string path)
    {
        directoryInfo = new DirectoryInfo(path);
        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        FileInfo[] files = directoryInfo.GetFiles("*.json");
        for (int i = 0; i < files.Length; i++)
        {
            this.files.Add(files[i]);
        }
		for (int i = 0; i < dirs.Length; i++)
		{
			subDirectories.Add(new ItemDirectory(dirs[i]));
		}
        this.path = path;
    }
    //kollar om det finns någon fils namn innehåller itemID.
    public FileInfo getFileContainingString(int itemID) {
        //gör så att itemID avrundar till 500 tal.
        int normID = (int)Mathf.Ceil((itemID + 1) / (Tools.ITEM_INTERVAL*1f)) * Tools.ITEM_INTERVAL;
        Debug.Log("finding: _" + normID);
        foreach (FileInfo file in files)
        {
            if (file.Name.Contains("_" + normID + ""))
            {
                return file;
            }
        }
        //rekrusiv funktion. 
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
        reader.Close();
        ItemDataAll ItemDataAll = itemDataConverter(itemID, fileContents);
        //Debug.Log("ITEM ID!!!: " + itemID);
        ItemVariables variables = ItemDataAll.generateVariables();
        itemValues.Add(itemID, variables);
        Debug.Log("dictionary size: " + itemValues.Count);
        return variables;
    }
    public ItemVariables getMobStats(int mobID) {
        if (itemValues.ContainsKey(mobID))
        {
            return itemValues[mobID];
        }
        FileInfo file = this.directory.getFileContainingString(mobID);
        StreamReader reader = file.OpenText();
        string fileContents = "";
        string line = "";
        while ((line = reader.ReadLine()) != null)
        {
            fileContents += line + Environment.NewLine;
        }
        reader.Close();
        ItemDataAll ItemDataAll = mobDataConverter(mobID, fileContents);
        ItemVariables variables = ItemDataAll.generateVariables();
        itemValues.Add(mobID, variables);
        return variables;
    }
    private ItemDataAll mobDataConverter(int itemID, string file) {
        ItemDataPots data = JsonUtility.FromJson<ItemDataPots>(file);
        data.parentItems = data.items;
        data = (ItemDataPots)getItemFromParent(data, itemID);
        return data;
    }
    private ItemDataAll itemDataConverter(int itemID, string file) {
        if (itemID.isItemType(e_itemTypes.USE)) {
            ItemDataPots data = JsonUtility.FromJson<ItemDataPots>(file);
            data.parentItems = data.items;
            data = (ItemDataPots)getItemFromParent(data,itemID);
            return data;
        }
        else if (itemID.isItemType(e_itemTypes.EQUIP))
        {
            ItemDataEquips data = JsonUtility.FromJson<ItemDataEquips>(file);
            data.parentItems = data.items;
            data = (ItemDataEquips)getItemFromParent(data, itemID);
            return data;
        }
		else if (itemID.isItemType(e_itemTypes.COIN))
		{
			ItemDataEtc data = JsonUtility.FromJson<ItemDataEtc>(file);
			data.parentItems = data.items;
            data = (ItemDataEtc)getItemFromParent(data, itemID);
			return data;
		}
        return null;
    }
    private ItemDataAll getItemFromParent(ItemDataAll data, int itemID) {
        return data.parentItems[itemID - data.startIndex];
    }
}
[Serializable]
public class ItemVariables {
    public Dictionary<string, int> variables_int = new Dictionary<string, int>();
    public Dictionary<string, string> variables_string = new Dictionary<string, string>();
    public Dictionary<string, float> variables_float = new Dictionary<string, float>();
    public string[] toShow;
    public int getInt(string varName) {
        return variables_int[varName];
    }
    public string getString(string varName)
    {
        return variables_string[varName];
    }
    public float getFloat(string varName)
    {
        return variables_float[varName];
    }
    public void addInt(string name, int value) {
        variables_int.Add(name, value);
    }
    public void addString(string name, string value)
    {
        variables_string.Add(name, value);
    }
    public void addFloat(string name, float value)
    {
        variables_float.Add(name, value);
    }
    public Dictionary<string, float> getFloats() {
        return variables_float;
    }
    public Dictionary<string, string> getStrings()
    {
        return variables_string;
    }
    public void setToShow(string[] toShow) {
        this.toShow = toShow;
    }
    public bool shouldShow(string item) {
        for (int i = 0; i < toShow.Length; i++) {
            if (toShow[i] == item)
                return true;
        }
        return false;
    }
    public Dictionary<string, int> getInts()
    {
        return variables_int;
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
    public ItemVariables generateVariables()
    {
        ItemVariables variables = new ItemVariables();
        foreach (FieldInfo field in this.GetType().GetFields())
        {
            if (field.FieldType == typeof(int)) {
                variables.addInt(field.Name,(int)field.GetValue(this));
            }
            if (field.FieldType == typeof(float))
            {
                variables.addFloat(field.Name, (int)field.GetValue(this));
            }
            if (field.FieldType == typeof(string))
            {
                variables.addString(field.Name, (string)field.GetValue(this));
            }
            if (field.FieldType == typeof(string[]) && field.Name == "show") {
                variables.setToShow((string[])field.GetValue(this));
            }
        }
        return variables;
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
    public string description;
    public string[] show;
    public string pathToDropModel;
}

[Serializable]
public class ItemDataEquips : ItemDataAll
{
    public ItemDataEquips[] items;
    public int id;
    public int luk,dex,str,Int,Matt,Watt;
    public string name;
    public int damage;
    public int price;
    public int imageIndex;
    public string description;
    public string pathToModel;
    public string pathToDropModel;
}

[Serializable]
public class ItemDataScrolls : ItemDataAll
{
    public int id;
    public string name;
    public int health;
    public int mana;
    public int imageIndex;
    public string description;
    public string[] show;
    public string pathToDropModel;
}
[Serializable]
public class ItemDataEtc : ItemDataAll
{
	public ItemDataEtc[] items;
    public int id;
    public string name;
    public int price;
    public int imageIndex;
    public string description;
    public string[] show;
    public string pathToModel;
    public string pathToDropModel;
}
