using BaldiEndless;
using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;

namespace CarnivalPackEndlessCompat
{
    [BepInPlugin("mtm101.rulerp.bbplus.carnivalpackendless", "Carnival Pack Endless Compat", "1.0.0.0")]
    [BepInDependency("mtm101.rulerp.baldiplus.endlessfloors")]
    public class CarnivalPackEndlessCompat : BaseUnityPlugin
    {

        void RegisterImportant()
        {
            EndlessFloorsPlugin.AddGeneratorAction(this.Info, GeneratorStuff);
        }

        void GeneratorStuff(GeneratorData data)
        {
            Character eenum = EnumExtensions.GetFromExtendedName<Character>("Zorp");
            Items eenum2 = EnumExtensions.GetFromExtendedName<Items>("CottonCandy");
            NPC npc = NPCMetaStorage.Instance.Get(eenum).value;
            ItemObject item = ItemMetaStorage.Instance.FindByEnum(eenum2).value;
            data.npcs.Add(new WeightedNPC { selection = npc, weight = 90 });
            data.items.Add(new WeightedItemObject() { selection = item, weight = 77});
        }

        void Awake()
        {
            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterImportant, true);
        }
    }
}
