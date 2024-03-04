using MTM101BaldAPI.Components;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarnivalPack
{
    public class Xorplee : NPC
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioManager audMan;
        public SoundObject doneSound;
        public SoundObject escapeSound;
        public Entity myEnt;
        public CustomSpriteAnimator animator;
        public AudioManager wahahAudMan;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public List<SoundObject> discoverSounds = new List<SoundObject>();


        public List<SoundObject> lostSounds = new List<SoundObject>();

        public List<SoundObject> goodSubjectSounds = new List<SoundObject>();
        public List<SoundObject> badSubjectSounds = new List<SoundObject>();
        public List<SoundObject> jammedSounds = new List<SoundObject>();

        public float timeRequiredToSeePlayer = 0.7f;

        public SpriteRenderer[] tractorBeams = new SpriteRenderer[0];
        public PlayerManager? currentTarget;

        public float timeBeforeNoSeeGiveUp = 10f;
        public float startSuckPower = 7f;
        public float endSuckPower = 36f;
        public float timeToReachMax = 24f;

        public int pointsToReward = 25;

        float timeUntilTractorBlink = 0.02f;

        bool tractorBlink = true;

        public float cooldownTime = 15f;
        public float standardSpeed = 22f;
        public float speedVariance = 10f;

        public Vector3 home;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Cell homeCell;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // copied directly from gotta sweep lol
        public bool IsHome
        {
            get
            {
                return ec.CellFromPosition(base.transform.position) == homeCell;
            }
        }

        private IEnumerator MoveSpriteByAmountEnumerator(float amount, float time)
        {
            float passed = 0f;
            Vector3 endPos = this.spriteRenderer[0].gameObject.transform.localPosition + (Vector3.up * amount);
            while (passed < time)
            {
                float delta = Time.deltaTime * ec.NpcTimeScale;
                passed += delta;
                this.spriteRenderer[0].gameObject.transform.localPosition += (Vector3.up * ((amount / time) * delta));
                yield return null;
            }
            this.spriteRenderer[0].gameObject.transform.localPosition = endPos;
            yield break;
        }

        public void MoveSpriteByAmount(float amount, float time)
        {
            StartCoroutine(MoveSpriteByAmountEnumerator(amount, time));
        }

        public override void Initialize()
        {
            base.Initialize();
            this.behaviorStateMachine.ChangeState(new Xorplee_Wander(this));
            home = transform.position;
            homeCell = ec.CellFromPosition(home);
            myEnt = this.GetComponent<Entity>();
            int tractorBeamCount = 13;
            tractorBeams = new SpriteRenderer[tractorBeamCount];
            for (int i = 0; i < tractorBeamCount; i++)
            {
                SpriteRenderer clone = GameObject.Instantiate<SpriteRenderer>(this.spriteRenderer[0]);
                clone.transform.parent = spriteRenderer[0].transform.parent;
                clone.gameObject.name = "Tractor" + i;
                clone.sprite = CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("Tractor" + Mathf.RoundToInt((float)i * (4f / tractorBeamCount)));
                clone.enabled = false;
                tractorBeams[i] = clone;
            }
            animator.animations.Add("Idle", new CustomAnimation<Sprite>(new Sprite[]
            {
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_idle_1"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_idle_2"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_idle_3"),
            }, 0.3f));
            animator.animations.Add("Jammed", new CustomAnimation<Sprite>(new Sprite[]
            {
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_jammed_1"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_jammed_2"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_jammed_3"),
            }, 0.3f));
            animator.animations.Add("Tract", new CustomAnimation<Sprite>(new Sprite[]
            {
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract1"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract2"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract3"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract4"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract5"),
            }, 1f));
            animator.animations.Add("TractBack", new CustomAnimation<Sprite>(new Sprite[]
            {
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract5"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract4"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract3"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract2"),
                CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract1"),
            }, 1f));
            animator.animations.Add("TractIdle", new CustomAnimation<Sprite>(new CustomAnimationFrame<Sprite>[]
            {
                    new CustomAnimationFrame<Sprite>(CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract5"), 0.2f),
                    new CustomAnimationFrame<Sprite>(CarnivalPackBasePlugin.Instance.assetMan.Get<Sprite>("xorp_tract4"), 0.05f)
            }));
            animator.SetDefaultAnimation("Idle", 1f);
        }

        public void BeginSuck(PlayerManager player)
        {
            currentTarget = player;
            PlayRandomSound(discoverSounds);
            behaviorStateMachine.ChangeState(new Xorplee_Suck(this));
            animator.Play("Tract", 1f);
            animator.SetDefaultAnimation("TractIdle",1f);
        }

        public void UpdateMoveSpeed(float mult = 1f)
        {
            float calculatedSpeed = (standardSpeed + (speedVariance * Mathf.Sin(ec.SurpassedGameTime * 1.5f))) * mult;
            Navigator.maxSpeed = calculatedSpeed;
            Navigator.SetSpeed(calculatedSpeed);
        }

        public void EndSuck() 
        {
            PlayRandomSound(lostSounds);
            behaviorStateMachine.ChangeState(new Xorplee_Wander(this));
            currentTarget = null;
            animator.Play("TractBack", 1f);
            animator.SetDefaultAnimation("Idle", 1f);
        }

        public void BecomeJammed()
        {
            PlayRandomSound(jammedSounds);
            behaviorStateMachine.ChangeState(new Xorplee_Jammed(this));
            animator.SetDefaultAnimation("Jammed", 1f);
        }

        public void PlayRandomSound(List<SoundObject> sounds)
        {
            audMan.PlayRandomAudio(sounds.ToArray());
        }

        public void UpdateTractor(Vector3? vec)
        {
            if (!vec.HasValue)
            {
                for (int i = 0; i < tractorBeams.Length; i++)
                {
                    tractorBeams[i].enabled = false;
                }
                return;
            }
            timeUntilTractorBlink -= Time.deltaTime * ec.NpcTimeScale;
            if (timeUntilTractorBlink <= 0)
            {
                tractorBlink = !tractorBlink;
                timeUntilTractorBlink = 0.02f;
            }
            for (int i = 0; i < tractorBeams.Length; i++)
            {
                tractorBeams[i].enabled = tractorBlink;
                float lerpPos = (((float)i + 1f) / (float)tractorBeams.Length);
                tractorBeams[i].gameObject.transform.position = Vector3.Lerp(transform.position, vec.Value, lerpPos * 0.97f);
            }
        }

        protected override void VirtualUpdate()
        {
            if (Mathf.Floor(navigator.Speed) == 0f)
            {
                wahahAudMan.volumeModifier = 0f;
            }
            else
            {
                wahahAudMan.volumeModifier = 1f;
                wahahAudMan.pitchModifier = navigator.Speed / 24f;
            }
        }

        public void Carry(Entity ent, bool happy)
        {
            this.PlayRandomSound(happy ? goodSubjectSounds : badSubjectSounds);
            behaviorStateMachine.ChangeState(new Xorplee_Carry(this, ent));
        }
    }

    public class Xorplee_StateBase : NpcState
    {
        public Xorplee_StateBase(Xorplee xxorp) : base(xxorp)
        {
            xorp = xxorp;
        }

        protected Xorplee xorp;
    }

    public class Xorplee_Carry : Xorplee_StateBase
    {
        protected Entity targetEnt;

        private MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);

        private bool initLookerState;

        public Xorplee_Carry(Xorplee xxorp, Entity ent) : base(xxorp)
        {
            targetEnt = ent;
            targetEnt.ExternalActivity.moveMods.Add(moveMod);
            ent.gameObject.transform.position = xorp.transform.position;
            xorp.animator.Play("TractBack", 1f);
            xorp.animator.SetDefaultAnimation("Idle", 1f);
            xorp.MoveSpriteByAmount(2.75f, 1f);
            //ent.SetBaseRotation(90);
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_TargetPosition(xorp, 63, xorp.home));
            Looker look = targetEnt.GetComponent<Looker>();
            if (look != null)
            {
                initLookerState = look.enabled;
            }
            targetEnt.SetTrigger(false);
            SetLookerStateIfExists(false);
        }

        public void SetLookerStateIfExists(bool state)
        {
            Looker look = targetEnt.GetComponent<Looker>();
            if (look != null)
            {
                if ((state && initLookerState) == false)
                {
                    look.Blink();
                }
                look.enabled = state && initLookerState;
            }
        }

        public override void Update()
        {
            base.Update();
            xorp.UpdateMoveSpeed(1.25f);
            moveMod.movementAddend = xorp.Navigator.Velocity.normalized * xorp.Navigator.speed * xorp.Navigator.Am.Multiplier;
        }

        public override void OnStateTriggerExit(Collider other)
        {
            base.OnStateTriggerExit(other);
            if (other.GetComponent<Entity>() == targetEnt)
            {
                base.ChangeNavigationState(new NavigationState_WanderRandom(xorp, 0));
                xorp.audMan.PlaySingle(xorp.escapeSound);
                xorp.behaviorStateMachine.ChangeState(new Xorplee_Wait(xorp, 2f));
            }
        }

        public override void Exit()
        {
            base.Exit();
            targetEnt.ExternalActivity.moveMods.Remove(moveMod);
            targetEnt.SetTrigger(true);
            SetLookerStateIfExists(true);
            xorp.MoveSpriteByAmount(-2.75f, 3f);
            //targetEnt.SetBaseRotation(0);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (!xorp.IsHome)
            {
                xorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(xorp.home);
                return;
            }
            xorp.audMan.PlaySingle(xorp.doneSound);
            if (targetEnt.GetType() == typeof(PlayerEntity))
            {
                Singleton<CoreGameManager>.Instance.AddPoints(xorp.pointsToReward, targetEnt.GetComponent<PlayerManager>().playerNumber, true);
                xorp.pointsToReward += 5;
            }
            xorp.behaviorStateMachine.ChangeState(new Xorplee_Wait(xorp, xorp.cooldownTime));
        }
    }

    public class Xorplee_Jammed : Xorplee_StateBase
    {

        public Xorplee_Jammed(Xorplee xxorp) : base(xxorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_TargetPosition(xorp, 63, xorp.home));
            xorp.animator.SetDefaultAnimation("Jammed", 1f);
        }

        public override void Update()
        {
            base.Update();
            xorp.UpdateMoveSpeed(0.95f);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (!xorp.IsHome)
            {
                xorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(xorp.home);
                return;
            }
            //xorp.audMan.PlaySingle(xorp.doneSound);
            xorp.behaviorStateMachine.ChangeState(new Xorplee_Wander(xorp));
        }
    }

    public class Xorplee_Wait : Xorplee_Wander
    {
        public float remainingTime;
        public override float wanderMult => 0.8f;
        public Xorplee_Wait(Xorplee xxorp, float time) : base(xxorp)
        {
            remainingTime = time;
            xorp.animator.Play("Idle", 1f);
        }

        public override void Update()
        {
            base.Update();
            remainingTime -= Time.deltaTime * xorp.ec.NpcTimeScale;
            if (remainingTime <= 0f)
            {
                xorp.behaviorStateMachine.ChangeState(new Xorplee_Wander(xorp));
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            playerSighted = true;
            timeSeeingPlayer = 0f;
        }
    }

    public class Xorplee_Suck : Xorplee_StateBase
    {
        public float timeWithoutSeeingPlayer = 0f;

        public float beamActiveTime = 0f;

        public MovementModifier moveMod = new MovementModifier(Vector3.zero, 0.96f);
        // todo: figure out this stupid mask!!
        public LayerMask myMask = ~LayerMask.GetMask("Ignore Raycast B", "Ignore Raycast");//new LayerMask() { value = 2195457 };

        private Vector3 lastPosition = Vector3.zero;

        public Entity? currentMoveModTarget;

        RaycastHit info;

        public Xorplee_Suck(Xorplee xxorp) : base(xxorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (xorp.currentTarget == null) throw new InvalidOperationException("Attempted to switch to Xorplee_Suck without target!");
            base.ChangeNavigationState(new NavigationState_TargetPlayer(xorp, 63, xorp.currentTarget.transform.position));
            lastPosition = xorp.currentTarget.transform.position;
            xorp.Navigator.maxSpeed = 2.5f;
            xorp.Navigator.SetSpeed(2.5f);
        }

        public override void Update()
        {
            base.Update();
            if (xorp.myEnt.Squished)
            {
                xorp.EndSuck();
                return;
            }
            timeWithoutSeeingPlayer += Time.deltaTime * xorp.ec.NpcTimeScale;
            beamActiveTime += Time.deltaTime * xorp.ec.NpcTimeScale;
            if (timeWithoutSeeingPlayer >= xorp.timeBeforeNoSeeGiveUp)
            {
                xorp.EndSuck();
                return;
            }
            if (xorp.currentTarget == null) return;
            if (currentMoveModTarget == null)
            {
                xorp.UpdateTractor(lastPosition);
            }
            else
            {
                xorp.UpdateTractor(currentMoveModTarget.transform.position);
            }
            if (Physics.SphereCast(xorp.transform.position, 0.8f, lastPosition - xorp.transform.position, out info, float.MaxValue, myMask.value, QueryTriggerInteraction.Ignore))
            {
                if (!info.collider)
                {
                    SwitchOrUpdateMoveTarget(null);
                    return;
                }
                Entity ent = info.collider.GetComponent<Entity>();
                if (!ent)
                {
                    SwitchOrUpdateMoveTarget(null);
                    return;
                }
                SwitchOrUpdateMoveTarget(ent);

            }
        }

        public override void OnStateTriggerEnter(Collider other)
        {
            Entity otherEnt = other.GetComponent<Entity>();
            if (otherEnt != null)
            {
                if ((otherEnt.gameObject.layer == LayerMask.NameToLayer("NPCs")) || (otherEnt.gameObject.layer == LayerMask.NameToLayer("Player")))
                {
                    xorp.Carry(otherEnt, otherEnt.GetComponent<PlayerManager>() != null);
                }
                else
                {
                    xorp.animator.Play("TractBack", 1f);
                    xorp.BecomeJammed();
                }
            }    
        }

        public void SwitchOrUpdateMoveTarget(Entity? newTarget)
        {
            if ((newTarget == null) && (currentMoveModTarget != null))
            {
                currentMoveModTarget.ExternalActivity.moveMods.Remove(moveMod);
                currentMoveModTarget = null;
            }
            if (newTarget != currentMoveModTarget)
            {
                if (currentMoveModTarget != null)
                {
                    currentMoveModTarget.ExternalActivity.moveMods.Remove(moveMod);
                }
                currentMoveModTarget = newTarget;
                if (currentMoveModTarget == null) return;
                currentMoveModTarget.ExternalActivity.moveMods.Add(moveMod);
            }
            if (currentMoveModTarget == null) return;
            float suckPower = Mathf.Lerp(xorp.startSuckPower, xorp.endSuckPower, Mathf.Min(beamActiveTime / xorp.timeToReachMax, 1f));
            moveMod.movementAddend = (currentMoveModTarget.transform.position - xorp.transform.position).normalized * -suckPower;
        }

        public override void Exit()
        {
            base.Exit();
            xorp.UpdateTractor(null);
            if (currentMoveModTarget != null)
            {
                currentMoveModTarget.ExternalActivity.moveMods.Remove(moveMod);
            }
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            xorp.Navigator.maxSpeed = 2.5f;
            xorp.Navigator.SetSpeed(2.5f);
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (xorp.currentTarget == player)
            {
                xorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(player.transform.position);
                lastPosition = player.transform.position;
                timeWithoutSeeingPlayer = 0f;
            }
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            xorp.Navigator.maxSpeed = 5f;
            xorp.Navigator.SetSpeed(5f);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            xorp.EndSuck();
            base.ChangeNavigationState(new NavigationState_WanderRandom(xorp, 0));
        }
    }

    public class Xorplee_Wander : Xorplee_StateBase
    {

        public float timeSeeingPlayer = 0f;
        public virtual float wanderMult => 1f;
        public bool playerSighted = false;
        public PlayerManager? lastSeenPlayer;

        public Xorplee_Wander(Xorplee xxorp) : base(xxorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_WanderRandom(xorp, 0));
            xorp.animator.SetDefaultAnimation("Idle", 1f);
        }

        public override void Update()
        {
            base.Update();
            xorp.UpdateMoveSpeed(wanderMult);
            if (!playerSighted)
            {
                timeSeeingPlayer = Mathf.Max(0f, timeSeeingPlayer - Time.deltaTime * xorp.ec.NpcTimeScale);
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerSighted(player);
            lastSeenPlayer = player;
            playerSighted = true;
            timeSeeingPlayer += Time.deltaTime * xorp.ec.NpcTimeScale;
            if ((timeSeeingPlayer >= xorp.timeRequiredToSeePlayer) && (!xorp.myEnt.Squished))
            {
                xorp.BeginSuck(player);
            }
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            lastSeenPlayer = null;
            playerSighted = false;
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            base.ChangeNavigationState(new NavigationState_WanderRandom(xorp, 0));
        }
    }
}
