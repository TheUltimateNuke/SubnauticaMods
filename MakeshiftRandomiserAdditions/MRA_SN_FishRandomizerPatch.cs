using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
                foreach (Type type in
            Assembly.GetAssembly(typeof(Creature)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Creature))))
                {
                    Creatures.Add(type.Name.ToLower());
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
                UWE.CoroutineHost.StartCoroutine(DoPatch(__instance));
            }
        }

        public static IEnumerator DoPatch(Knife __instance)
        {
            var position = default(Vector3);
            GameObject gameObject = null;
            UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref gameObject, ref position, true);

            if (gameObject)
            {
                LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();

                if (liveMixin)
                {
                    GameObject targetObject = liveMixin.gameObject;

                    var isAlive = liveMixin.IsAlive();
                    var className = targetObject.name.Replace("(Clone)", "");
                    var isCreature = Creatures.IndexOf(className.ToLower()) > -1;

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

                            Enum.TryParse(creature, true, out TechType type);

                            if (type == TechType.None)
                            {
                                type = getRandomCreature();
                            }

                            return type;
                        }

                        GameObject newObject = null;

                        while (newObject == null)
                        {
                            var creatureType = getRandomCreature();

                            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(creatureType);

                            yield return task;

                            newObject = task.GetResult();
                        }

                        var newCreature = newObject.GetComponent<LiveMixin>();
                        newCreature.health = Mathf.Clamp(health, 0, newCreature.maxHealth);

                        var instantiatedObject = UnityEngine.Object.Instantiate(newCreature, parent);
                        instantiatedObject.transform.position = pos;
                        instantiatedObject.transform.rotation = rotation;
                    }
                }
            }
        }
    }
}
