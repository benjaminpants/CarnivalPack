using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.AssetTools.SpriteSheets;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CarnivalPack
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInPlugin("mtm101.rulerp.bbplus.carnivalpackroot", "Carnival Pack Root Mod", "1.4.0.0")]
    public class CarnivalPackBasePlugin : BaseUnityPlugin
    {
        public static CarnivalPackBasePlugin Instance;

        public Dictionary<string, CustomAnimation<Sprite>> zorpsterAnimations;

        public ConfigEntry<bool> youtuberModeEnabled;

        public AssetManager assetMan = new AssetManager();

        public static RoomCategory ZorpCat = EnumExtensions.ExtendEnum<RoomCategory>("ZorpRoom");


        void AddAudioFolderToAssetMan(Color subColor, params string[] path)
        {
            string[] paths = Directory.GetFiles(Path.Combine(path));
            for (int i = 0; i < paths.Length; i++)
            {
                assetMan.Add<SoundObject>("Aud_" + Path.GetFileNameWithoutExtension(paths[i]), ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(paths[i]), Path.GetFileNameWithoutExtension(paths[i]), SoundType.Voice, subColor));
            }
        }

        void AddSpriteFolderToAssetMan(string prefix = "", float pixelsPerUnit = 40f, params string[] path)
        {
            string[] paths = Directory.GetFiles(Path.Combine(path));
            for (int i = 0; i < paths.Length; i++)
            {
                assetMan.Add<Sprite>(prefix + Path.GetFileNameWithoutExtension(paths[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(paths[i]), pixelsPerUnit));
            }
        }

        void RegisterImportant()
        {

            StandardDoorMats doorMats = ObjectCreators.CreateDoorDataObject("ZorpDoor", AssetLoader.TextureFromMod(this, "Map", "ZorpDoor_Open.png"), AssetLoader.TextureFromMod(this, "Map", "ZorpDoor_Closed.png"));
            // create the room asset
            RoomAsset ZorpRoom = ScriptableObject.CreateInstance<RoomAsset>();
            ZorpRoom.name = "Zorpster_Room";
            ZorpRoom.hasActivity = false;
            ZorpRoom.activity = new ActivityData();
            ZorpRoom.ceilTex = assetMan.Get<Texture2D>("ZorpCeil");
            ZorpRoom.florTex = assetMan.Get<Texture2D>("ZorpFloor");
            ZorpRoom.wallTex = assetMan.Get<Texture2D>("ZorpWall");
            ZorpRoom.doorMats = doorMats;
            ZorpRoom.potentialDoorPositions = new List<IntVector2>() { new IntVector2(0, 0) };
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(0, 0),
                type = 12
            });
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(0, 1),
                type = 9
            });
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(1, 0),
                type = 4
            });
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(1, 1),
                type = 1
            });
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(2, 0),
                type = 6
            });
            ZorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(2, 1),
                type = 3
            });
            ZorpRoom.standardLightCells.Add(new IntVector2(0, 0));
            ZorpRoom.entitySafeCells.Add(new IntVector2(2, 1));
            ZorpRoom.eventSafeCells.Add(new IntVector2(0, 0));
            ZorpRoom.eventSafeCells.Add(new IntVector2(0, 0));
            ZorpRoom.lightPre = MTM101BaldiDevAPI.roomAssetMeta.Get("Room_ReflexOffice_0").value.lightPre;
            ZorpRoom.color = new Color(172f / 255f, 0f, 252f / 255f);
            ZorpRoom.category = ZorpCat;
            MTM101BaldiDevAPI.roomAssetMeta.Add(new RoomAssetMeta(this.Info, ZorpRoom));
            assetMan.Add<RoomAsset>("Zorp_Room", ZorpRoom);


            Zorpster Zorp = new NPCBuilder<Zorpster>(Info)
                .SetName("Zorpster")
                .SetEnum("Zorp")
                .SetAirborne()
                .IgnorePlayerOnSpawn()
                .AddLooker()
                .AddTrigger()
                .AddSpawnableRoomCategories(ZorpCat)
                .AddPotentialRoomAsset(ZorpRoom, 100)
                .SetPoster(AssetLoader.TextureFromMod(this, "zorpster_poster.png"), "PST_PRI_Zorpster1", "PST_PRI_Zorpster2")
                .Build();

            Zorp.spriteRenderer[0].gameObject.transform.localPosition += Vector3.up;
            Zorp.audMan = Zorp.GetComponent<AudioManager>();
            Zorp.wahahAudMan = Zorp.gameObject.AddComponent<PropagatedAudioManager>();
            Zorp.wahahAudMan.ReflectionSetVariable("soundOnStart", new SoundObject[] { assetMan.Get<SoundObject>("Zorpster_Sound_Idle") });
            Zorp.wahahAudMan.ReflectionSetVariable("loopOnStart", true);
            Zorp.spriteRenderer[0].sprite = assetMan.Get<Sprite>("Zorpster_Idle");
            Zorp.discoverSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Zorp_Discover1"), assetMan.Get<SoundObject>("Aud_Zorp_Discover2") });
            Zorp.lostSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Zorp_Lost1"), assetMan.Get<SoundObject>("Aud_Zorp_Lost2"), assetMan.Get<SoundObject>("Aud_Zorp_Lost3") });
            Zorp.goodSubjectSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Zorp_Correct1"), assetMan.Get<SoundObject>("Aud_Zorp_Correct2"), assetMan.Get<SoundObject>("Aud_Zorp_Correct3") });
            Zorp.badSubjectSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Zorp_Wrong1"), assetMan.Get<SoundObject>("Aud_Zorp_Wrong2")});
            Zorp.jammedSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Zorp_Jammed1"), assetMan.Get<SoundObject>("Aud_Zorp_Jammed2"), assetMan.Get<SoundObject>("Aud_Zorp_Jammed3"), assetMan.Get<SoundObject>("Aud_Zorp_Jammed4") });
            Zorp.doneSound = assetMan.Get<SoundObject>("Aud_Zorp_Done1");
            Zorp.escapeSound = assetMan.Get<SoundObject>("Aud_Zorp_Escape");

            // ANIMATOR!
            CustomSpriteAnimator animator = Zorp.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = Zorp.spriteRenderer[0];
            Zorp.animator = animator;

            assetMan.Add<Zorpster>("Zorpster", Zorp);

            ItemObject cottonCandy = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_CottonCandy", "Desc_CottonCandy")
                .SetSprites(assetMan.Get<Sprite>("CottonCandySmall"), assetMan.Get<Sprite>("CottonCandyBig"))
                .SetEnum("CottonCandy")
                .SetShopPrice(480)
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_CottonCandy>()
                .SetMeta(ItemFlags.Persists, new string[0])
                .Build();
            assetMan.Add<ItemObject>("CottonCandy", cottonCandy);
        }

        void AddNPCs(string floorName, int floorNumber, SceneObject sceneObject)
        {
            if (!youtuberModeEnabled.Value)
            {
                if (floorName == "F1")
                {
                    sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Zorpster"), weight = 100 });
                    sceneObject.MarkAsNeverUnload();
                }
                if (floorName == "F2")
                {
                    sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Zorpster"), weight = 25 }); // surprise zorpster
                }
            }
            else
            {
                if (floorName == "F1")
                {
                    sceneObject.forcedNpcs = sceneObject.levelObject.forcedNpcs.AddToArray(assetMan.Get<NPC>("Zorpster"));
                    sceneObject.additionalNPCs = Mathf.Max(sceneObject.levelObject.additionalNPCs - 1, 0);
                }
            }
            if (floorName.StartsWith("F"))
            {
                sceneObject.levelObject.potentialItems = sceneObject.levelObject.potentialItems.AddItem(new WeightedItemObject() { selection = assetMan.Get<ItemObject>("CottonCandy"), weight = 80 }).ToArray();
                sceneObject.MarkAsNeverUnload();
            }
            if (floorNumber >= 1)
            {
                sceneObject.shopItems = sceneObject.shopItems.AddItem(new WeightedItemObject() { selection = assetMan.Get<ItemObject>("CottonCandy"), weight = 75 }).ToArray();
                sceneObject.MarkAsNeverUnload();
            }

        }

        IEnumerator PreLoadBulk()
        {
            yield return 2;
            yield return "Loading Zorpster Sprites...";
            zorpsterAnimations = SpriteSheetLoader.LoadAsepriteAnimationsFromFile(Path.Combine(AssetLoader.GetModPath(this), "Zorpster.json"), 40f, Vector2.one / 2f);
            //AddSpriteFolderToAssetMan("", 40f, AssetLoader.GetModPath(this), "ZorpAnim");
            yield return "Loading Zorpster Audio...";
            AddAudioFolderToAssetMan(new Color(107f / 255f, 193f / 255f, 27 / 255f), AssetLoader.GetModPath(this), "ZorpLines");
            yield break;
        }

        void Awake()
        {
            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.carnivalpackroot");
            harmony.PatchAllConditionals();
            assetMan.Add<Texture2D>("Texture_Zorpster_Idle", AssetLoader.TextureFromMod(this, "ZorpPlaceholder.png"));
            assetMan.Add<Sprite>("Zorpster_Idle", AssetLoader.SpriteFromTexture2D(assetMan.Get<Texture2D>("Texture_Zorpster_Idle"), 40));
            assetMan.Add<SoundObject>("Zorpster_Sound_Idle", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "weirdwahah.wav"), "Sfx_WeirdWahah", SoundType.Effect, Color.white));
            assetMan.Add<Texture2D>("ZorpWall", AssetLoader.TextureFromMod(this, "Map", "ZorpWall.png"));
            assetMan.Add<Texture2D>("ZorpCeil", AssetLoader.TextureFromMod(this, "Map", "ZorpCeil.png"));
            assetMan.Add<Texture2D>("ZorpFloor", AssetLoader.TextureFromMod(this, "Map", "ZorpFloor.png"));
            assetMan.Add<Sprite>("Tractor1", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor1.png"), 30));
            assetMan.Add<Sprite>("Tractor2", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor2.png"), 30));
            assetMan.Add<Sprite>("Tractor3", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor3.png"), 30));
            assetMan.Add<Sprite>("Tractor4", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor4.png"), 30));
            assetMan.Add<Sprite>("CottonCandySmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "CottonCandySmall.png"), 25f));
            assetMan.Add<Sprite>("CottonCandyBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "CottonCandyBig.png"), 50f));
            assetMan.Add<Sprite>("Staminometer_Cotton", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Staminometer_Cotton.png"), 50f));
            AssetLoader.LocalizationFromMod(this);
            //AddSpriteFolderToAssetMan("", 40f, AssetLoader.GetModPath(this), "ZorpAnim");
            //AddAudioFolderToAssetMan(new Color(107f/255f,193f/255f,27/255f), AssetLoader.GetModPath(this), "ZorpLines");
            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterImportant, false);
            LoadingEvents.RegisterOnLoadingScreenStart(Info, PreLoadBulk());
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);
            Instance = this;

            youtuberModeEnabled = Config.Bind<bool>("General", "Youtuber Mode", false, "If true, Zorpster will always appear on Floor 1.");

            ModdedSaveGame.AddSaveHandler(new CarnivalPackSaveGameIO());
        }
    }

    public class CarnivalPackSaveGameIO : ModdedSaveGameIOBinary
    {
        public override PluginInfo pluginInfo => CarnivalPackBasePlugin.Instance.Info;

        public override void Load(BinaryReader reader)
        {
            reader.ReadByte();
        }

        public override void Reset()
        {
            
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write((byte)0);
        }

        public override string[] GenerateTags()
        {
            if (CarnivalPackBasePlugin.Instance.youtuberModeEnabled.Value)
            {
                return new string[1] { "YoutuberMode" };
            }
            return new string[0];
        }

        public override string DisplayTags(string[] tags)
        {
            if (tags.Contains("YoutuberMode"))
            {
                return "Youtuber Mode";
            }
            return "Standard Mode";
        }
    }
}
