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

        MethodInfo methodInfo = this.GetType().GetMethod(splitMessage[0]);

        if(methodInfo == null) {
            commandNotFound();
            return;
        }

        switch (splitMessage.Length) {
            case 1:
            methodInfo.Invoke(this, null);
            break;
            case 2:
            methodInfo.Invoke(this, new object[] { splitMessage[1] });
            break;
            case 3:
            methodInfo.Invoke(this, new object[] { splitMessage[1], splitMessage[2] });
            break;
            case 4:
            methodInfo.Invoke(this, new object[] { splitMessage[1], splitMessage[2], splitMessage[3] });
            break;
            case 5:
            methodInfo.Invoke(this, new object[] { splitMessage[1], splitMessage[2], splitMessage[3], splitMessage[4] });
            break;

        }

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
