using MTM101BaldAPI.Components;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarnivalPack
{
    public class Zorpster : NPC
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
        public float timeToReachMax = 26f;

        public int pointsToReward = 25;

        float timeUntilTractorBlink = 0.02f;

        bool tractorBlink = true;

        public float cooldownTime = 25f;
        public float standardSpeed = 22f;
        public float speedVariance = 10f;

        public float nonPlayerSuckAddition = 8f;

        private float currentOffset = 0f;

        public Vector3 spriteStartingPosition;
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

        IEnumerator? currentAnimEnumerator;

        private IEnumerator UpdateSpritePositionEnumerator(float time)
        {
            float passed = 0f;
            Vector3 startPos = spriteRenderer[0].gameObject.transform.localPosition;
            Vector3 endPos = spriteStartingPosition + (Vector3.up * currentOffset);
            while (passed < time)
            {
                float delta = Time.deltaTime * ec.NpcTimeScale;
                passed += delta;
                this.spriteRenderer[0].gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, passed / time);//+= (Vector3.up * ((amount / time) * delta));
                yield return null;
            }
            this.spriteRenderer[0].gameObject.transform.localPosition = endPos;
            currentAnimEnumerator = null;
            yield break;
        }

        public void MoveSpriteByAmount(float amount, float time)
        {
            currentOffset += amount;
            if (currentAnimEnumerator != null)
            {
                StopCoroutine(currentAnimEnumerator);
            }
            currentAnimEnumerator = UpdateSpritePositionEnumerator(time);
            StartCoroutine(currentAnimEnumerator);
        }

        public void MoveSpriteToBase(float time)
        {
            MoveSpriteByAmount(-currentOffset, time);
        }

        public override void Initialize()
        {
            base.Initialize();
            spriteStartingPosition = spriteRenderer[0].gameObject.transform.localPosition;
            home = transform.position;
            homeCell = ec.CellFromPosition(home);
            myEnt = this.GetComponent<Entity>();
            int tractorBeamCount = 15;
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
            animator.animations = CarnivalPackBasePlugin.Instance.zorpsterAnimations;
            animator.SetDefaultAnimation("Idle", 1f);
            this.behaviorStateMachine.ChangeState(new Zorpster_Wander(this));
        }

        public void BeginSuck(PlayerManager player)
        {
            currentTarget = player;
            PlayRandomSound(discoverSounds);
            behaviorStateMachine.ChangeState(new Zorpster_Suck(this));
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
            behaviorStateMachine.ChangeState(new Zorpster_Wander(this));
            currentTarget = null;
            animator.Play("TractBack", 1f);
            animator.SetDefaultAnimation("Idle", 1f);
        }

        public void BecomeJammed()
        {
            PlayRandomSound(jammedSounds);
            behaviorStateMachine.ChangeState(new Zorpster_Jammed(this));
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
            if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
            {
                timeUntilTractorBlink -= Time.deltaTime * ec.NpcTimeScale;
                if (timeUntilTractorBlink <= 0)
                {
                    tractorBlink = !tractorBlink;
                    timeUntilTractorBlink = 0.02f;
                }
            }
            else
            {
                tractorBlink = true;
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
            behaviorStateMachine.ChangeState(new Zorpster_Carry(this, ent));
        }
    }

    public class Zorpster_StateBase : NpcState
    {
        public Zorpster_StateBase(Zorpster xZorp) : base(xZorp)
        {
            Zorp = xZorp;
        }

        protected Zorpster Zorp;
    }

    public class Zorpster_Carry : Zorpster_StateBase
    {
        protected Entity targetEnt;

        private MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);

        private bool initLookerState;

        public Zorpster_Carry(Zorpster xZorp, Entity ent) : base(xZorp)
        {
            targetEnt = ent;
            targetEnt.ExternalActivity.moveMods.Add(moveMod);
            ent.gameObject.transform.position = Zorp.transform.position;
            Zorp.animator.Play("TractBack", 1f);
            Zorp.animator.SetDefaultAnimation("Idle", 1f);
            Zorp.MoveSpriteByAmount(2.75f, 1f);
            //ent.SetBaseRotation(90);
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_TargetPosition(Zorp, 63, Zorp.home));
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
            Zorp.UpdateMoveSpeed(1.25f);
            moveMod.movementAddend = Zorp.Navigator.Velocity.normalized * Zorp.Navigator.speed * Zorp.Navigator.Am.Multiplier;
        }

        public override void OnStateTriggerExit(Collider other)
        {
            base.OnStateTriggerExit(other);
            if (other.GetComponent<Entity>() == targetEnt)
            {
                base.ChangeNavigationState(new NavigationState_WanderRandom(Zorp, 0));
                Zorp.audMan.PlaySingle(Zorp.escapeSound);
                Zorp.behaviorStateMachine.ChangeState(new Zorpster_Wait(Zorp, 2f));
            }
        }

        public override void Exit()
        {
            base.Exit();
            targetEnt.ExternalActivity.moveMods.Remove(moveMod);
            targetEnt.SetTrigger(true);
            SetLookerStateIfExists(true);
            Zorp.MoveSpriteToBase(3f);
            //targetEnt.SetBaseRotation(0);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (!Zorp.IsHome)
            {
                Zorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(Zorp.home);
                return;
            }
            Zorp.audMan.PlaySingle(Zorp.doneSound);
            if (targetEnt.GetType() == typeof(PlayerEntity))
            {
                Singleton<CoreGameManager>.Instance.AddPoints(Zorp.pointsToReward, targetEnt.GetComponent<PlayerManager>().playerNumber, true);
                Zorp.pointsToReward += 5;
            }
            Zorp.behaviorStateMachine.ChangeState(new Zorpster_Wait(Zorp, Zorp.cooldownTime));
        }
    }

    public class Zorpster_Jammed : Zorpster_StateBase
    {

        public Zorpster_Jammed(Zorpster xZorp) : base(xZorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_TargetPosition(Zorp, 63, Zorp.home));
            Zorp.animator.SetDefaultAnimation("Jammed", 1f);
        }

        public override void Update()
        {
            base.Update();
            Zorp.UpdateMoveSpeed(0.95f);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (!Zorp.IsHome)
            {
                Zorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(Zorp.home);
                return;
            }
            //Zorp.audMan.PlaySingle(Zorp.doneSound);
            Zorp.behaviorStateMachine.ChangeState(new Zorpster_Wander(Zorp));
        }
    }

    public class Zorpster_Wait : Zorpster_Wander
    {
        public float remainingTime;
        public override float wanderMult => 0.8f;
        public Zorpster_Wait(Zorpster xZorp, float time) : base(xZorp)
        {
            remainingTime = time;
            Zorp.animator.Play("Idle", 1f);
        }

        public override void Update()
        {
            base.Update();
            remainingTime -= Time.deltaTime * Zorp.ec.NpcTimeScale;
            if (remainingTime <= 0f)
            {
                Zorp.behaviorStateMachine.ChangeState(new Zorpster_Wander(Zorp));
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            playerSighted = true;
            timeSeeingPlayer = 0f;
        }
    }

    public class Zorpster_Suck : Zorpster_StateBase
    {
        public float timeWithoutSeeingPlayer = 0f;

        public float beamActiveTime = 0f;

        public MovementModifier moveMod = new MovementModifier(Vector3.zero, 0.96f);
        // todo: figure out this stupid mask!!
        public LayerMask myMask = ~LayerMask.GetMask("Ignore Raycast B", "Ignore Raycast");//new LayerMask() { value = 2195457 };
        public LayerMask myMaskIgnoreEntities = ~LayerMask.GetMask("Ignore Raycast B", "Ignore Raycast", "Player", "NPCs", "StandardEntities");//new LayerMask() { value = 2195457 };

        private Vector3 lastPosition = Vector3.zero;

        public Entity? currentMoveModTarget;

        RaycastHit info;

        public Zorpster_Suck(Zorpster xZorp) : base(xZorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (Zorp.currentTarget == null) throw new InvalidOperationException("Attempted to switch to Zorpster_Suck without target!");
            base.ChangeNavigationState(new NavigationState_TargetPlayer(Zorp, 63, Zorp.currentTarget.transform.position));
            lastPosition = Zorp.currentTarget.transform.position;
            Zorp.Navigator.maxSpeed = 2.5f;
            Zorp.Navigator.SetSpeed(2.5f);
        }

        public override void Update()
        {
            base.Update();
            if (Zorp.myEnt.Squished)
            {
                Zorp.EndSuck();
                return;
            }
            timeWithoutSeeingPlayer += Time.deltaTime * Zorp.ec.NpcTimeScale;
            beamActiveTime += Time.deltaTime * Zorp.ec.NpcTimeScale;
            if (timeWithoutSeeingPlayer >= Zorp.timeBeforeNoSeeGiveUp)
            {
                Zorp.EndSuck();
                return;
            }
            if (Zorp.currentTarget == null) return;
            if (currentMoveModTarget == null)
            {
                Zorp.UpdateTractor(lastPosition);
            }
            else
            {
                Zorp.UpdateTractor(currentMoveModTarget.transform.position);
            }
            if (Physics.SphereCast(Zorp.transform.position, 0.8f, lastPosition - Zorp.transform.position, out info, float.MaxValue, myMask.value, QueryTriggerInteraction.Ignore))
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
                    Zorp.Carry(otherEnt, otherEnt.GetComponent<PlayerManager>() != null);
                }
                else
                {
                    Zorp.animator.Play("TractBack", 1f);
                    Zorp.BecomeJammed();
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
            float suckPower = Mathf.Lerp(Zorp.startSuckPower, Zorp.endSuckPower, Mathf.Min(beamActiveTime / Zorp.timeToReachMax, 1f));
            if (!(currentMoveModTarget is PlayerEntity))
            {
                suckPower += Zorp.nonPlayerSuckAddition;
            }
            moveMod.movementAddend = (currentMoveModTarget.transform.position - Zorp.transform.position).normalized * -suckPower;
        }

        public override void Exit()
        {
            base.Exit();
            Zorp.UpdateTractor(null);
            if (currentMoveModTarget != null)
            {
                currentMoveModTarget.ExternalActivity.moveMods.Remove(moveMod);
            }
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            if (player.Tagged) return;
            Zorp.Navigator.maxSpeed = 2.5f;
            Zorp.Navigator.SetSpeed(2.5f);
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (player.Tagged)
            {
                if (Zorp.Navigator.maxSpeed < 5f)
                {
                    PlayerLost(player);
                }
                return;
            }
            if (Zorp.currentTarget == player)
            {
                Zorp.behaviorStateMachine.CurrentNavigationState.UpdatePosition(player.transform.position);
                if (Physics.SphereCast(player.transform.position, 0.6f, player.transform.position - Zorp.transform.position, out info, float.MaxValue, myMaskIgnoreEntities.value, QueryTriggerInteraction.Ignore))
                {
                    lastPosition = info.point;
                }
                else
                {
                    lastPosition = player.transform.position;
                }
                timeWithoutSeeingPlayer = 0f;
            }
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            Zorp.Navigator.maxSpeed = 5f;
            Zorp.Navigator.SetSpeed(5f);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            Zorp.EndSuck();
            base.ChangeNavigationState(new NavigationState_WanderRandom(Zorp, 0));
        }
    }

    public class Zorpster_Wander : Zorpster_StateBase
    {

        public float timeSeeingPlayer = 0f;
        public virtual float wanderMult => 1f;
        public bool playerSighted = false;
        public PlayerManager? lastSeenPlayer;

        public Zorpster_Wander(Zorpster xZorp) : base(xZorp)
        {
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_WanderRandom(Zorp, 0));
            Zorp.animator.SetDefaultAnimation("Idle", 1f);
        }

        public override void Update()
        {
            base.Update();
            Zorp.UpdateMoveSpeed(wanderMult);
            if (!playerSighted)
            {
                timeSeeingPlayer = Mathf.Max(0f, timeSeeingPlayer - Time.deltaTime * Zorp.ec.NpcTimeScale);
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerSighted(player);
            if (player.Tagged)
            {
                if (player == lastSeenPlayer)
                {
                    if (lastSeenPlayer != null)
                    {
                        PlayerLost(player);
                    }
                    return;
                }
            }
            lastSeenPlayer = player;
            playerSighted = true;
            timeSeeingPlayer += Time.deltaTime * Zorp.ec.NpcTimeScale;
            if ((timeSeeingPlayer >= Zorp.timeRequiredToSeePlayer) && (!Zorp.myEnt.Squished))
            {
                Zorp.BeginSuck(player);
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
            base.ChangeNavigationState(new NavigationState_WanderRandom(Zorp, 0));
        }
    }
}
