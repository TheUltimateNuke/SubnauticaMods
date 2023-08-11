using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static VFXParticlesPool;

namespace MRASubnautica
{
    /// <summary>
    /// Basically a port of the legacy mod Fish Randomizer by Nyx.
    /// </summary>
    public static class MRA_SN_FishRandomizer
    {
        public static List<string> Creatures = new List<string>();

        [HarmonyPatch(typeof(Player))]
        public static class Player_Patch
        {
            [HarmonyPatch(nameof(Player.Awake))]
            [HarmonyPostfix]
            public static void Postfix(Player __instance)
            {
                foreach (Type Type in Assembly.GetAssembly(typeof(Creature)).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(Creature))))
                {
                    Creatures.Add(Type.Name);
                }
            }
        }


        [HarmonyPatch(typeof(Knife))]
        public static class Knife_Patch
        {
            [HarmonyPatch(nameof(Knife.OnToolUseAnim))]
            [HarmonyPostfix]
            public static void Postfix(Knife __instance, GUIHand hand) 
            {
                MRA_SN.logger.LogInfo("Postfix of Knife.OnToolUseAnim " + __instance);
                UWE.CoroutineHost.StartCoroutine(DoPatch(__instance));
            }
        }

        public static IEnumerator DoPatch(Knife __instance)
        {
            var position = default(Vector3);
            GameObject gameObject = null;
            UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref gameObject, ref position, true);

            MRA_SN.logger.LogInfo("Recognized tool use anim. checking if it found a gameobject...");

            if (gameObject)
            {
                LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();

                if (liveMixin)
                {
                    GameObject targetObject = liveMixin.gameObject;

                    var isAlive = liveMixin.IsAlive();
                    var className = targetObject.name.Replace("(Clone)", "");
                    var isCreature = (Creatures.IndexOf(className) > -1);

                    if (isAlive && isCreature)
                    {
                        var pos = liveMixin.transform.position;
                        var rotation = liveMixin.transform.rotation;
                        var health = liveMixin.health;
                        var parent = liveMixin.transform.parent;

                        UnityEngine.Object.Destroy(liveMixin.gameObject);

                        TechType getRandomCreature()
                        {
                            var random = new System.Random();
                            var rand = random.Next(0, Creatures.Count);

                            var creature = Creatures[rand];

                            // exceptions
                            if (creature == "SandShark")
                                creature = "Sandshark";

                            if (creature == "Garryfish")
                                creature = "GarryFish";

                            if (creature == "Holefish")
                                creature = "HoleFish";

                            if (creature == "CrabSnake")
                                creature = "Crabsnake";

                            if (creature == "BladderFish")
                                creature = "Bladderfish";

                            if (creature == "JellyRay")
                                creature = "Jellyray";

                            //De-Extinction checks

                            if (creature == "TriangleFish")
                                creature = "Trianglefish";

                            Enum.TryParse(creature, out TechType type);

                            if (type == TechType.None)
                            {
                                MRA_SN.logger.LogError("Couldn't fetch " + creature + " as random creature!");
                                type = getRandomCreature();
                            }

                            return type;
                        }

                        var creatureType = getRandomCreature();

                        MRA_SN.logger.LogInfo("Type Chosen: " + creatureType);

                        var task = new TaskResult<GameObject>();
                        yield return CraftData.GetPrefabForTechTypeAsync(creatureType, true, task);

                        var newObject = task.Get();
                        var newCreature = newObject.GetComponent<LiveMixin>();
                        newCreature.health = health;

                        var instantiatedObject = UnityEngine.Object.Instantiate(newCreature, parent);
                        instantiatedObject.transform.position = pos;
                        instantiatedObject.transform.rotation = rotation;
                    }
                }
            }
            else
            {
                MRA_SN.logger.LogError("Did not find gameObject from tool use anim.");
            }
        }
    }
}
