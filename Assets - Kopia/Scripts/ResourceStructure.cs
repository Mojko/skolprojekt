﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_Objects
{
    SYSTEM_RESPAWNER,
    MONSTER_SQUIRRLE,
    PARTICLE_DEATH,
    PARTICLE_GROUNDDUST,
    SKILL_PREFAB,
}

public class ResourceStructure {
    public static Dictionary<e_Objects, string> paths = new Dictionary<e_Objects, string>();

    public ResourceStructure()
    {
        paths.Add(e_Objects.MONSTER_SQUIRRLE, "Prefabs/Meshes/E_Squirrle");
        paths.Add(e_Objects.PARTICLE_DEATH, "Particles/Hit");
        paths.Add(e_Objects.PARTICLE_GROUNDDUST, "Particles/ImpactOnGround");
        paths.Add(e_Objects.SYSTEM_RESPAWNER, "System/Respawner");
        paths.Add(e_Objects.SKILL_PREFAB, "Particles/Skills/Skill");
    }

    public static string getPathForObject(e_Objects id)
    {
        return paths[id];
    }
    public static GameObject getGameObjectFromPath(string path)
    {
        return (GameObject)Resources.Load(path);
    }
    public static GameObject getGameObjectFromObject(e_Objects id)
    {
        string path = paths[id];
        if(path != null) {
            return (GameObject)Resources.Load(paths[id]);
        }
        string error = "Could not find specified Object in paths Dictionary (Assets/Scripts/Game.cs)";
        Debug.LogError(error);
        return null;
    }
}
