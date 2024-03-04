using AlmostEngine;
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
        public float oldStaminaRise;
        void Start()
        {
            pm = this.GetComponent<PlayerManager>();
            oldStaminaRise = pm.plm.staminaRise;
            pm.plm.staminaRise = 0f;
            pm.plm.runSpeed += 5;
            pm.plm.walkSpeed -= 6;
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
                pm.plm.staminaRise = oldStaminaRise;
                pm.plm.runSpeed -= 5;
                pm.plm.walkSpeed += 6;
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
