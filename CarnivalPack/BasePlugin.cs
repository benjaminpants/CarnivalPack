using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CarnivalPack
{
    [BepInPlugin("mtm101.rulerp.bbplus.carnivalpackroot", "Carnival Pack Root Mod", "0.0.0.0")]
    public class CarnivalPackBasePlugin : BaseUnityPlugin
    {
        public static CarnivalPackBasePlugin Instance;

        public AssetManager assetMan = new AssetManager();

        public static RoomCategory xorpCat = EnumExtensions.ExtendEnum<RoomCategory>("XorpRoom");


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
            Character xorpEnum = EnumExtensions.ExtendEnum<Character>("Zorp");
            PosterObject xorpPoster = ObjectCreators.CreateCharacterPoster(AssetLoader.TextureFromMod(this, "kxorplee_poster.png"), "PST_PRI_Xorplee1", "PST_PRI_Xorplee2");
            Xorplee xorp = ObjectCreators.CreateNPC<Xorplee>("Xorp", xorpEnum, xorpPoster, spawnableRooms: new RoomCategory[] { xorpCat }, usesHeatMap: false, hasLooker: true);
            xorp.spriteRenderer[0].gameObject.transform.localPosition += Vector3.up;
            xorp.audMan = xorp.GetComponent<AudioManager>();
            xorp.wahahAudMan = xorp.gameObject.AddComponent<PropagatedAudioManager>();
            xorp.wahahAudMan.ReflectionSetVariable("soundOnStart", new SoundObject[] { assetMan.Get<SoundObject>("Xorpee_Sound_Idle") });
            xorp.wahahAudMan.ReflectionSetVariable("loopOnStart", true);
            xorp.spriteRenderer[0].sprite = assetMan.Get<Sprite>("Xorplee_Idle");
            xorp.discoverSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Xorp_Discover1"), assetMan.Get<SoundObject>("Aud_Xorp_Discover2") });
            xorp.lostSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Xorp_Lost1"), assetMan.Get<SoundObject>("Aud_Xorp_Lost2"), assetMan.Get<SoundObject>("Aud_Xorp_Lost3") });
            xorp.goodSubjectSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Xorp_Correct1"), assetMan.Get<SoundObject>("Aud_Xorp_Correct2"), assetMan.Get<SoundObject>("Aud_Xorp_Correct3") });
            xorp.badSubjectSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Xorp_Wrong1"), assetMan.Get<SoundObject>("Aud_Xorp_Wrong2")});
            xorp.jammedSounds.AddRange(new SoundObject[] { assetMan.Get<SoundObject>("Aud_Xorp_Jammed1"), assetMan.Get<SoundObject>("Aud_Xorp_Jammed2"), assetMan.Get<SoundObject>("Aud_Xorp_Jammed3"), assetMan.Get<SoundObject>("Aud_Xorp_Jammed4") });
            xorp.doneSound = assetMan.Get<SoundObject>("Aud_Xorp_Done1");
            xorp.escapeSound = assetMan.Get<SoundObject>("Aud_Xorp_Escape");
            xorp.ReflectionSetVariable("ignorePlayerOnSpawn", true);

            // ANIMATOR!
            CustomSpriteAnimator animator = xorp.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = xorp.spriteRenderer[0];
            xorp.animator = animator;



            assetMan.Add<Xorplee>("Xorplee", xorp);
            NPCMetaStorage.Instance.Add(new NPCMetadata(this.Info, new NPC[] { xorp }, "Xorp", NPCFlags.Standard));
            // create the room asset
            Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            RoomAsset xorpRoom = ScriptableObject.CreateInstance<RoomAsset>();
            xorpRoom.hasActivity = false;
            xorpRoom.activity = new ActivityData();
            xorpRoom.ceilTex = assetMan.Get<Texture2D>("XorpCeil");
            xorpRoom.florTex = assetMan.Get<Texture2D>("XorpFloor");
            xorpRoom.wallTex = assetMan.Get<Texture2D>("XorpWall");
            xorpRoom.doorMats = Resources.FindObjectsOfTypeAll<StandardDoorMats>().Where(x => x.name == "DefaultDoorSet").First();
            xorpRoom.potentialDoorPositions = new List<IntVector2>() { new IntVector2(0, 0) };
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(0, 0),
                type = 12
            });
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(0, 1),
                type = 9
            });
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(1, 0),
                type = 4
            });
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(1, 1),
                type = 1
            });
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(2, 0),
                type = 6
            });
            xorpRoom.cells.Add(new CellData()
            {
                pos = new IntVector2(2, 1),
                type = 3
            });
            xorpRoom.standardLightCells.Add(new IntVector2(0,0));
            xorpRoom.entitySafeCells.Add(new IntVector2(2, 1));
            xorpRoom.eventSafeCells.Add(new IntVector2(0, 0));
            xorpRoom.eventSafeCells.Add(new IntVector2(0, 0));
            xorpRoom.lightPre = MTM101BaldiDevAPI.roomAssetMeta.Get("Room_ReflexOffice_0").value.lightPre;
            xorpRoom.color = new Color(154f / 255f, 288f / 255f, 213f / 255f);
            xorpRoom.category = xorpCat;
            assetMan.Add<RoomAsset>("Xorp_Room", xorpRoom);
            xorp.potentialRoomAssets = new WeightedRoomAsset[]
            {
                new WeightedRoomAsset()
                {
                    selection = xorpRoom,
                    weight = 100
                }
            };

            Items cottonEnum = EnumExtensions.ExtendEnum<Items>("CottonCandy");
            ItemObject cottonCandy = ObjectCreators.CreateItemObject("Itm_CottonCandy", "Desc_CottonCandy", assetMan.Get<Sprite>("CottonCandySmall"), assetMan.Get<Sprite>("CottonCandyBig"), cottonEnum, 160, 40);
            ITM_CottonCandy candy = new GameObject().AddComponent<ITM_CottonCandy>();
            cottonCandy.item = candy;
            assetMan.Add<ItemObject>("CottonCandy", cottonCandy);
            candy.gameObject.name = "CottonCandy Object";
            ItemMetaData meta = new ItemMetaData(this.Info, cottonCandy);
            cottonCandy.AddMeta(meta);
            DontDestroyOnLoad(candy.gameObject);
        }

        void AddNPCs(string floorName, int floorNumber, LevelObject floorObject)
        {
            if (floorName == "F1")
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Xorplee"), weight = 100 });
                floorObject.MarkAsNeverUnload();
            }
            else if (floorName == "END")
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Xorplee"), weight = 75});
                floorObject.MarkAsNeverUnload();
            }
            if (floorName == "F1" || floorName == "F2" || floorName == "F3")
            {
                floorObject.items = floorObject.items.AddItem(new WeightedItemObject() { selection = assetMan.Get<ItemObject>("CottonCandy"), weight = 100 }).ToArray();
                floorObject.MarkAsNeverUnload();
            }
            if (floorName == "F2" || floorName == "F3")
            {
                floorObject.shopItems = floorObject.shopItems.AddItem(new WeightedItemObject() { selection = assetMan.Get<ItemObject>("CottonCandy"), weight = 85 }).ToArray();
            }
        }

        void Awake()
        {
            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.carnivalpackroot");
            harmony.PatchAllConditionals();
            assetMan.Add<Texture2D>("Texture_Xorplee_Idle", AssetLoader.TextureFromMod(this, "xorplee.png"));
            assetMan.Add<Sprite>("Xorplee_Idle", AssetLoader.SpriteFromTexture2D(assetMan.Get<Texture2D>("Texture_Xorplee_Idle"), 45));
            assetMan.Add<SoundObject>("Xorpee_Sound_Idle", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "weirdwahah.wav"), "Sfx_WeirdWahah", SoundType.Effect, Color.white));
            assetMan.Add<Texture2D>("XorpWall", AssetLoader.TextureFromMod(this, "Map", "XorpWall.png"));
            assetMan.Add<Texture2D>("XorpCeil", AssetLoader.TextureFromMod(this, "Map", "XorpCeil.png"));
            assetMan.Add<Texture2D>("XorpFloor", AssetLoader.TextureFromMod(this, "Map", "XorpFloor.png"));
            assetMan.Add<Sprite>("Tractor1", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor1.png"), 30));
            assetMan.Add<Sprite>("Tractor2", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor2.png"), 30));
            assetMan.Add<Sprite>("Tractor3", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor3.png"), 30));
            assetMan.Add<Sprite>("Tractor4", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "tractor4.png"), 30));
            assetMan.Add<Sprite>("CottonCandySmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "CottonCandySmall.png"), 25f));
            assetMan.Add<Sprite>("CottonCandyBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "CottonCandyBig.png"), 50f));
            assetMan.Add<Sprite>("Staminometer_Cotton", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Staminometer_Cotton.png"), 50f));
            AddSpriteFolderToAssetMan("", 40f, AssetLoader.GetModPath(this), "XorpAnim");
            AddAudioFolderToAssetMan(new Color(107f/255f,193f/255f,27/255f), AssetLoader.GetModPath(this), "XorpLines");
            LoadingEvents.RegisterOnAssetsLoaded(RegisterImportant, false);
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);
            Instance = this;
        }
    }
}
