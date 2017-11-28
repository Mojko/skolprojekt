using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Command
{
    public string command;
    public Command(string command)
    {
        this.command = command;
    }
}

public class CommandManager {

    private List<Command> commands = new List<Command>();
    private Player player;

    public CommandManager(Player player)
    {
        commands.Add(new Command("addItem"));
        commands.Add(new Command("spawnMob"));
        this.player = player;
    }
    public void listenForCommand(string command)
    {
        command = command.TrimStart("/".ToCharArray());
        string[] splitMessage = command.Split(null);
        // /addItem 
        MethodInfo methodInfo = this.GetType().GetMethod(splitMessage[0]);

        if(methodInfo == null) {
            commandNotFound();
            return;
        }
        object[] obj = new object[splitMessage.Length - 1];
        for (int i = 0; i < obj.Length; i++) {
            obj[i] = splitMessage[i + 1];
        }
        methodInfo.Invoke(this, obj);

    }
    public void help()
    {
        for(int i = 0; i < commands.Count; i++) {
            
        }
    }
    public void addItem(string id, string amount)
    {
        int idInteger = int.Parse(id);
        int amountInteger = int.Parse(amount);
        //player.getNetwork().sendMessage("Added item " + id + " to your inventory", MessageTypes.SERVER, "");
    }
    public void spawnMob(string id, string amount)
    {
        int idInteger = int.Parse(id);
        int amountInteger = int.Parse(amount);
        player.getNetwork().spawnMobFromClient(idInteger, amountInteger);
        player.getNetwork().sendMessage("Spawned mob " + id, MessageTypes.SERVER, "");
    }
    public void commandNotFound(){
        player.getNetwork().sendMessage("Command not found, type /help for all commands", MessageTypes.SERVER, "");
    }
}
