using AlmostEngine;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CarnivalPack
{
    public class CottonCandyGraphicReverter : MonoBehaviour
    {
        public CottonCandyManager myManager;
        public Sprite originalSprite;
        public Image staminoMeter;
        void Update()
        {
            if (myManager == null)
            {
                staminoMeter.sprite = originalSprite;
                Destroy(this);
            }
        }
    }

    public class CottonCandyManager : MonoBehaviour
    {
        public Sprite? originalStaminoSprite;
        public Image? staminoMeter;
        public PlayerManager? pm;
        //public float oldStaminaRise;
        ValueModifier staminaRaiseModifier = new ValueModifier(0f);
        ValueModifier runSpeedModifier = new ValueModifier(1f, 5f);
        ValueModifier walkSpeedModifier = new ValueModifier(1f, -6f);
        void Start()
        {
            pm = this.GetComponent<PlayerManager>();
            PlayerMovementStatModifier pmsm = pm.GetMovementStatModifier();
            pmsm.AddModifier("staminaRise", staminaRaiseModifier);
            pmsm.AddModifier("runSpeed", runSpeedModifier);
            pmsm.AddModifier("walkSpeed", walkSpeedModifier);
        }

        void Update()
        {
            if ((pm == null) || (staminoMeter == null))
            {
                Destroy(this);
                return;
            }
            if (pm.plm.stamina <= 0f)
            {
                pm.GetMovementStatModifier().RemoveModifier(staminaRaiseModifier);
                pm.GetMovementStatModifier().RemoveModifier(runSpeedModifier);
                pm.GetMovementStatModifier().RemoveModifier(walkSpeedModifier);
                staminoMeter.sprite = originalStaminoSprite;
                Destroy(this);
            }
        }
    }

    public class ITM_CottonCandy : Item
    {
        public override bool Use(PlayerManager pm)
        {
            if (pm.gameObject.GetComponent<CottonCandyManager>() != null) return false; // we already doing performing the cotton candy!
            pm.plm.stamina = Math.Max(pm.plm.stamina, pm.plm.staminaMax);
            CottonCandyManager cm = pm.gameObject.AddComponent<CottonCandyManager>();
            HudManager hudMan = Singleton<CoreGameManager>.Instance.GetHud(0);
            Image stamImage = hudMan.transform.Find("Staminometer").Find("Overlay").GetComponent<Image>();
            cm.originalStaminoSprite = stamImage.sprite;
            cm.staminoMeter = stamImage;
            stamImage.sprite = CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("Staminometer_Cotton");
            CottonCandyGraphicReverter reverter = stamImage.gameObject.AddComponent<CottonCandyGraphicReverter>();
            reverter.myManager = cm;
            reverter.originalSprite = cm.originalStaminoSprite;
            reverter.staminoMeter = cm.staminoMeter;
            return true;
        }
    }
}
