using BaldiEndless;
using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;

namespace CarnivalPackEndlessCompat
{
    [BepInPlugin("mtm101.rulerp.bbplus.carnivalpackendless", "Carnival Pack Endless Compat", "0.0.0.0")]
    [BepInDependency("mtm101.rulerp.baldiplus.endlessfloors")]
    public class CarnivalPackEndlessCompat : BaseUnityPlugin
    {

        void RegisterImportant()
        {
            EndlessFloorsPlugin.AddGeneratorAction(this.Info, GeneratorStuff);
        }

        void GeneratorStuff(GeneratorData data)
        {
            NPC npc = NPCMetaStorage.Instance.Get(EnumExtensions.GetFromExtendedName<Character>("Zorp")).value;
            ItemObject item = ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("CottonCandy")).value;
            data.npcs.Add(new WeightedNPC { selection = npc, weight = 90 });
            data.items.Add(new WeightedItemObject() { selection = item, weight = 60});
        }

        void Awake()
        {
            LoadingEvents.RegisterOnAssetsLoaded(RegisterImportant, true);
        }
    }
}
