using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
using System.Text;
using NPCManager;
using UnityEngine.Networking;
public class NPCCompiler{
    public NPCConversationManager objManager;

    public void init(NetworkConnection player)
    {
        objManager = new NPCConversationManager(player);
    }

    public Func<NPCConversationManager, int, bool> compileNPC(int npcID, NetworkConnection player)
    {
        objManager = new NPCConversationManager(player);
        string codeString2 = @"
            using System;
            using NPCManager;
            class NpcScript{ 
                public static bool active(NPCConversationManager manager,int state) { 
                    if(state == 0){
                        manager.sendNext(""|t|0.9|t|OMG|t|orgSpeed|t| Hello fellow stranger. I need some help, can you please collect five |I|1000|I| and ten |I|1002|I|"");
                        return false;
                    }
                    return true;
                }
            }
            ";
        //kompilerar stringen och gör den til lett Assembly objekt.
        Assembly assembly = compNpc(codeString2, false);
        //skapar ett objekt av klassen i codeString2 stringen.
        object runTimeType = assembly.CreateInstance("NpcScript");
        //hämtar metoden OnRun ur classen.
        MethodInfo method = runTimeType.GetType().GetMethod("active");

        //Skapar en Action vilket betyder att metoden ovan inte returnar något, om den skulle returna något skulle man skriva Func istället.
        //CreateDelegate skapar en Delegate vilket blir castat till en Action. Action tar emot så många parametrar som fanns i metoden. vilet är ett objekt och en int.
        Func<NPCConversationManager, int, bool> func = (Func<NPCConversationManager, int, bool>)Delegate.CreateDelegate(typeof(Func<NPCConversationManager, int, bool>), method);
        //här kör den OnRun metoden.
        func.Invoke(objManager, 0);
        return func;
    }

    private Assembly compNpc(string src, bool isFile)
    {
        string code;
        //kollar om det är en fil källa eller en vanlig string.
        if (isFile)
            code = File.ReadAllText(src);
        else
            code = src;

        //skapar CSharpCodeProvider objekt och parametern säger vilken version som ska användas.
        CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>
            {
                {"CompilerVersion", "v3.5"}
            });
        //skapar CompilerParameters objektet.
        CompilerParameters param = new CompilerParameters();
        //gör så att det inte skapas en .exe, säger att den ska köras i minnet och visa bugs som kan komma.
        param.GenerateExecutable = false;
        param.GenerateInMemory = true;
        param.IncludeDebugInformation = true;


        //går igenom alla Asemblies som finns och lägger till dom i ReferencedAssemblies.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            //Debug.Log("assembly.Location: " + assembly.Location);
            param.ReferencedAssemblies.Add(assembly.Location);
        }
        //skapar CompilerResult som ska kompila CompilerParameters och CSharpCodeProvider till en assembly objekt.
        CompilerResults result = provider.CompileAssemblyFromSource(param, code);
        //kollar om det fanns några errors.
        if (result.Errors.HasErrors)
        {
            StringBuilder builder = new StringBuilder();
            foreach (CompilerError error in result.Errors)
            {
                builder.AppendFormat("Error ({0}): {1}\n", error.ErrorNumber, error.ErrorText);
            }
            throw new System.Exception(builder.ToString());
        }
        //kompilerar stringen till ett c# objekt och skivkar tillbaka den.
        return result.CompiledAssembly;
    }

}
namespace NPCManager
{
    public class NPCConversationManager
    {
        NetworkConnection client;
        public NPCConversationManager(NetworkConnection client) {
            this.client = client;
        }
        public void sendNext(string text)
        {
            NPCInteractPacket packet = new NPCInteractPacket();
            packet.npcText = text;
            packet.type = NPCTalkType.NEXT;
            Debug.Log("sending to client");
            Debug.Log("debugged npc worked her em8");
            NetworkServer.SendToClient(client.connectionId,PacketTypes.NPC_INTERACT, packet);
        }
        public void sendYesNo(string text)
        {
            Debug.Log(text);
        }
    }
}