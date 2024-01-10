using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ZooAbyss.Bosses
{
    public enum WormSegmentType
    {
        /// <summary>
        /// The head segment for the worm. A boss worm can have multiple "heads" towing their following segments
        /// </summary>
        Head,
        /// <summary>
        /// The body segment. Follows the segment in front of it.
        /// </summary>
        Body,
        /// <summary>
        /// The tail segment. Follows the segment in front of it.
        /// </summary>
        Tail,
        /// <summary>
        /// The boss worm entity who manages the worm.  
        /// </summary>
        Entity
    }

    /// <summary>
    /// The base class for separating boss worm enemies.
    /// </summary>
    public abstract class BossWorm : ModNPC    // a boss worm like the Eater of Worlds
    {
        /*  ai[] usage:
		 *  
		 *  ai[0] = "follower" segment, the segment that's following this segment
		 *  ai[1] = "following" segment, the segment that this segment is following
		 *  
		 *  localAI[0] = used when syncing changes to collision detection
		 *  localAI[1] = checking if Init() was called
		 */

        /// <summary>
        /// Which type of segment this NPC is considered to be
        /// </summary>
        public abstract WormSegmentType SegmentType { get; }

        /// <summary>
        /// The maximum velocity for the NPC
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// The rate at which the NPC gains velocity
        /// </summary>
        public float Acceleration { get; set; }

        /// <summary>
        /// The NPC instance of the original head segment for this worm. (it has access to all of the worms segments and stores the boss worms total health)
        /// </summary>
        public BossWormEntity BossWormEntityNPC { get; set; }

        /// <summary>
        /// The NPC instance of the segment that this segment is following (ai[1]).  For head segments, this property always returns <see langword="null"/>.
        /// </summary>
        public NPC FollowingNPC => SegmentType == WormSegmentType.Head ? null : Main.npc[(int)NPC.ai[1]];

        /// <summary>
        /// The NPC instance of the segment that is following this segment (ai[0]).  For tail segment, this property always returns <see langword="null"/>.
        /// </summary>
        public NPC FollowerNPC => SegmentType == WormSegmentType.Tail ? null : Main.npc[(int)NPC.ai[0]];

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (SegmentType == WormSegmentType.Entity)   return false; // the BossWormEntity shall not be visible by anything
            else   return null;
        }

        public bool startDespawning;
        public bool startDespawningPropagated;

        public sealed override bool PreAI()
        {
            if (NPC.localAI[1] == 0)
            {
                NPC.localAI[1] = 1f;
                Init();
            }

            if (SegmentType == WormSegmentType.Head)
            {
                HeadAI();

                if (!NPC.HasValidTarget)
                {
                    NPC.TargetClosest(true);

                    // If the NPC is a boss and it has no target, force it to fall to the underworld quickly
                    if (!NPC.HasValidTarget )
                    {
                        NPC.velocity.Y += 2f;

                        MoveSpeed = 1000f;

                        if (!startDespawning)
                        {
                            startDespawning = true; // set here only for the heads, as they are towing the bodies and tails

                            // Despawn after 90 ticks (1.5 seconds) if the NPC gets far enough away
                            NPC.EncourageDespawn(90);
                        }
                    }
                }
            }
            else if (SegmentType == WormSegmentType.Body)
            {
                BodyAI();
            }
            else if (SegmentType == WormSegmentType.Tail)
            {
                TailAI();
            }
            else if (SegmentType == WormSegmentType.Entity)
            {
                EntityAI();
            }

            return true;
        }

        // Not visible to public API, but is used to indicate what AI to run
        internal virtual void HeadAI() { }

        internal virtual void BodyAI() { }

        internal virtual void TailAI() { }

        internal virtual void EntityAI() { }

        /// <summary>
        /// This user implementable method is getting called when the 
        /// </summary>
        public abstract void Init();
    }

    /// <summary>
    /// The base class for the "all worm" managing entity of the boss worm. It doesn't appear as a NPC but carries the "boss" status and dies when all segments are dead
    /// </summary>
    public abstract class BossWormEntity : BossWorm
    {
        public sealed override WormSegmentType SegmentType => WormSegmentType.Entity;

        /// <summary>
        /// The NPCID or ModContent.NPCType for the head segment NPC.<br/>
        /// This property is only used if <see cref="HasCustomBodySegments"/> returns <see langword="false"/>.
        /// </summary>
        public abstract int HeadType { get; }

        /// <summary>
        /// The NPCID or ModContent.NPCType for the body segment NPCs.<br/>
        /// This property is only used if <see cref="HasCustomBodySegments"/> returns <see langword="false"/>.
        /// </summary>
        public abstract int BodyType { get; }

        /// <summary>
        /// The NPCID or ModContent.NPCType for the tail segment NPC.<br/>
        /// This property is only used if <see cref="HasCustomBodySegments"/> returns <see langword="false"/>.
        /// </summary>
        public abstract int TailType { get; }

        /// <summary>
        /// Whether the NPC uses a different types for his body segments...which is always false, because all the boss worm segments will be of the same type
        /// </summary>
        public virtual bool HasCustomBodySegments => false;

        /// <summary>
        /// The minimum amount of segments expected, including the head and tail segments
        /// </summary>
        public int MinSegmentLength { get; set; }

        /// <summary>
        /// The maximum amount of segments expected, including the head and tail segments
        /// </summary>
        public int MaxSegmentLength { get; set; }

        /// <summary>
        /// The array where all worm segments are stored (only the main head will have this array filled)
        /// </summary>
        public NPC[] Segment { get; set; }

        /// <summary>
        /// The total current life of the whole boss worm (only the main head will have this value written)
        /// </summary>
        public int TotalCurrentLife { get; set; }

        /// <summary>
        /// The total maximal life of the whole boss worm (only the main head will have this value written)
        /// </summary>
        public int TotalMaxLife { get; set; }

        /// <summary>
        /// The BosWormHead where the BossWormEntity is attached to
        /// </summary>
        private NPC HeadStickedTo;

        /// <summary>
        /// A flag that indicates, that recently a despawn happened. Initiates a check for denying the loot
        /// </summary>
        private bool DespawnHappend = false;

        /// <summary>
        /// If true, the BossWormEntity will despawn, denying it's loot
        /// </summary>
        private bool DenyLoot = false;

        /// <summary>
        /// Override this method to use custom body-spawning code.<br/>
        /// This method only runs if <see cref="HasCustomBodySegments"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="segmentCount">How many body segments are expected to be spawned</param>
        /// <returns>The whoAmI of the most-recently spawned NPC, which is the result of calling <see cref="NPC.NewNPC(Terraria.DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/></returns>
        public virtual int SpawnBodySegments(int segmentCount)
        {
            // Defaults to just returning this NPC's whoAmI, since the tail segment uses the return value as its "following" NPC index
            return NPC.whoAmI;
        }

        /// <summary>
        /// Spawns the head, body and tail segments of the worm.
        /// </summary>
        /// <param name="source">The spawn source</param>
        /// <param name="type">The ID of the segment NPC to spawn</param>
        /// <param name="latestNPC">The whoAmI of the most-recently spawned segment NPC in the worm, including the head</param>
        /// <returns></returns>
        protected int SpawnSegment(IEntitySource source, int type, int latestNPC, BossWormEntity EntityNPC)
        {
            // We spawn a new NPC, setting latestNPC to the newer NPC, whilst also using that same variable
            // to set the parent of this new NPC. The parent of the new NPC (may it be a tail or body part)
            // will determine the movement of this new NPC.
            // Under there, we also set the realLife value of the new NPC, because of what is explained above.
            int previousSegment = latestNPC;
            int newSegment = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC);

            Main.npc[previousSegment].ai[0] = newSegment; //set the just created NPC as the "follower" segment of the last created segment. E.g. the just created NPC follows the previous segment

            (Main.npc[newSegment].ModNPC as BossWorm).BossWormEntityNPC = EntityNPC; // write the managing entity for later reference

            //NPC latest = Main.npc[latestNPC]; --> not needed because no life will be shared
            // NPC.realLife is the whoAmI of the NPC that the spawned NPC will share its health with
            //latest.realLife = NPC.whoAmI; --> not needed because no life will be shared

            return newSegment;
        }

        internal override void EntityAI()
        {
            EntityAI_SpawnSegments();

            CheckLife();

            if (DespawnHappend && !DenyLoot)
            {
                if (CheckDenyLoot())
                {
                    DenyLoot = true; //prevent killing the entity when the worm despawns
                    this.NPC.timeLeft = 90; // let entity despawn
                }
            }

            EntityAI_StickToHead();

            if (TotalCurrentLife <= 0) CheckIfDead();
        }
        private void EntityAI_SpawnSegments()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // So, we start the AI off by checking if the managing Entity reference is already written.
                // As this happens only after this method is run once, it means this is the first update of the method.
                // Since this is the first update, we can safely assume we need to spawn the the noss worm (head + bodies + tail).
                bool hasEntityNotWritten = (this.BossWormEntityNPC is null);
                if (hasEntityNotWritten)
                {
                    // So, here we assign the NPC.realLife value.
                    // The NPC.realLife value is mainly used to determine which NPC loses life when we hit this NPC.
                    // We don't want every single piece of the worm to have its own HP pool, so this is a neat way to fix that.
                    //NPC.realLife = NPC.whoAmI; --> "realLife" not needed because no life will be shared

                    // at first we spawn the head
                    IEntitySource source = NPC.GetSource_FromAI(); //Martin: I don't really understand this...does it refer to the spawning source?
                    int latestNpcIdx = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, HeadType, NPC.whoAmI); // latestNPC is also going to be used in SpawnSegment() and I'll explain it there some more.

                    BossWormEntityNPC = this; // writing this value will let make sure this method will not be called again
                    (Main.npc[latestNpcIdx].ModNPC as BossWorm).BossWormEntityNPC = BossWormEntityNPC; // write the managing entity to the head for later reference

                    // Here we determine the length of the worm, randomly.
                    int randomWormLength = Main.rand.Next(MinSegmentLength, MaxSegmentLength + 1);
                    randomWormLength = (randomWormLength < 2) ? 2 : (randomWormLength > 200) ? 200 : randomWormLength; // 2 <= randomWormLength <= 200
                    int bodySegmentCount = randomWormLength - 2; // "-2" because there the head and the tail aren't body segments

                    // initialize the Segment array that will be used for calulating the worms health
                    Segment = new NPC[randomWormLength];
                    Segment[0] = Main.npc[latestNpcIdx]; // store head as first element in segment array
                    HeadStickedTo = Segment[0]; //choose the head as the NPC where the Entity sticks to
                    int segmentIdx = 1; // for writing all the other segements

                    if (HasCustomBodySegments) // --> false for this boss worm
                    {
                        // Call the method that'll handle spawning the body segments
                        latestNpcIdx = SpawnBodySegments(bodySegmentCount);
                    }
                    else
                    {
                        // Spawn the body segments like usual
                        while (bodySegmentCount > 0)
                        {
                            latestNpcIdx = SpawnSegment(source, BodyType, latestNpcIdx, BossWormEntityNPC);
                            bodySegmentCount--;

                            Segment[segmentIdx] = Main.npc[latestNpcIdx]; // add this segment to the array
                            segmentIdx++;
                        }
                    }

                    // Spawn the tail segment
                    latestNpcIdx = SpawnSegment(source, TailType, latestNpcIdx, BossWormEntityNPC);
                    Segment[segmentIdx] = Main.npc[latestNpcIdx]; // add the tail to the array

                    NPC.netUpdate = true;

                    // Ensure that all of the segments could spawn.  If they could not, despawn the worm entirely
                    #region checkSpawnComplete
                    int count = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC n = Main.npc[i];

                        if (n.active && (n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
                            count++;
                    }

                    if (count != randomWormLength)
                    {
                        // Unable to spawn all of the segments... kill the worm
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];

                            if (n.active && (n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
                            {
                                n.active = false;
                                n.netUpdate = true;
                            }
                        }
                    }
                    #endregion

                    // Set the player target for good measure
                    NPC.TargetClosest(true);

                    // initialize the current and max life
                    TotalCurrentLife = CheckLife();
                    TotalMaxLife = TotalCurrentLife; // the boss worm just spawned, he is still at max life
                }
            }
        }
        /// <summary>
        /// Calculated the BossWorms life and corrects any irregularities in it's segments
        /// </summary>
        public virtual int CheckLife()
        {
            int currentLife = 0;
            for (int i = 0; i < BossWormEntityNPC.Segment.Length; i++)
            {
                if (BossWormEntityNPC.Segment[i].active) //reading a bool is more memory efficient than reading and comparing an int
                {
                    if (BossWormEntityNPC.Segment[i].life <= 0) // I didn't want to believe it, but yes sometimes aa segment has negative life!....maybe this happens due to lag?
                    {
                        BossWormEntityNPC.Segment[i].life = 0; // kill this abomination!...again? :-P
                        BossWormEntityNPC.Segment[i].checkDead();
                        BossWormEntityNPC.Segment[i].active = false;
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, BossWormEntityNPC.Segment[i].whoAmI, -1f);
                    }
                    currentLife += BossWormEntityNPC.Segment[i].life;

                    if (((BossWormEntityNPC.Segment[i].ModNPC as BossWorm).startDespawning) &&
                        !((BossWormEntityNPC.Segment[i].ModNPC as BossWorm).startDespawningPropagated))
                    {
                        PropagateDespawn(BossWormEntityNPC.Segment[i]);
                    }
                }
            }
            TotalCurrentLife = currentLife;

            return TotalCurrentLife;
        }

        /// <summary>
        /// Let's the BossWormEntity move alongside a head, so in the end the treasure bag drops nicely where the last segment was killed :-)
        /// </summary>
        private void EntityAI_StickToHead()
        {
            // find an active BossWormHead and stick to it if the first head died
            if ((HeadStickedTo.life <= 0) && // the sticked to NPC is dead
                 (Segment.Length > 0)) // just to be 100% sure the data is there
            {
                for (int i = 0; i < Segment.Length; i++)
                {
                    if ((Segment[i].active) &&
                         (Segment[i].ModNPC.Type == HeadType))
                    {
                        HeadStickedTo = Segment[i];
                        Main.NewText($"StickToHead: {i}");
                        break;
                    }
                }
            }
            if (HeadStickedTo is not null) this.NPC.position = HeadStickedTo.position;
        }

        /// <summary>
        /// If a head wants to despawn, propagate this to its followers
        /// </summary>
        private void PropagateDespawn(NPC startSegment)
        {
            bool followerExist = ((startSegment.ModNPC as BossWorm).FollowerNPC is not null); // the "startDespawning" is always set by heads, so only look at the followers
            NPC propagateNPC = startSegment; // init
            Main.NewText($"Despawn: Start:{startSegment.whoAmI}");
            while (followerExist)
            {
                propagateNPC = (propagateNPC.ModNPC as BossWorm).FollowerNPC; // get the next follower

                // set despawn values
                (propagateNPC.ModNPC as BossWorm).startDespawning = true;
                (propagateNPC.ModNPC as BossWorm).startDespawningPropagated = true; //mark as "done"
                propagateNPC.EncourageDespawn(90);

                // update WHILE condition
                followerExist = ((propagateNPC.ModNPC as BossWorm).FollowerNPC is not null);

                Main.NewText($"Despawn: Propagated:{propagateNPC.whoAmI}");
            }

            // finish
            (startSegment.ModNPC as BossWorm).startDespawningPropagated = true; //mark as "done"
            ((startSegment.ModNPC as BossWorm).BossWormEntityNPC as BossWormEntity).DespawnHappend = true;
        }

        /// <summary>
        /// Checks if the recently ordered to despawn segements were the only ones left in the BossWorm. If so, deny loot from dropping
        /// </summary>
        private bool CheckDenyLoot()
        {
            bool denyLootLocal = true; //init
            for (int i = 0; i < Segment.Length; i++)
            {
                if (Segment[i].ModNPC.Type == HeadType && // looking at heads is sufficient, as they are towing the bodies and tails
                    Segment[i].active)
                {
                    denyLootLocal &= (Segment[i].ModNPC as BossWorm).startDespawningPropagated;
                    Main.NewText($"DenyLoot: HeadNPC: {Segment[i].whoAmI}   |   Active:{Segment[i].active}   |   Propagated:{(Segment[i].ModNPC as BossWorm).startDespawningPropagated}");
                }
            }
            Main.NewText($"DenyLoot: Sum:{denyLootLocal}");
            return denyLootLocal;
        }

        /// <summary>
        /// Checks if all of the BossWorm segments are dead and kills the BossWormEntity if so
        /// </summary>
        public virtual bool CheckIfDead()
        {
            bool someSegmentStillAlive = false;
            for (int i = 0; i < BossWormEntityNPC.Segment.Length; i++)
            {
                someSegmentStillAlive |= BossWormEntityNPC.Segment[i].active;
            }
            Main.NewText($"CheckIfDead: SegmentsAlive: {someSegmentStillAlive}   |   DenyLoot{DenyLoot}");
            if (!someSegmentStillAlive) // No BossWorm segment alive anymore
            {
                if (DenyLoot) // no loot
                {
                    //despawn the entity
                    //BossWormEntityNPC.NPC.active = false;
                    startDespawning = true;
                    return true;
                }
                else if (BossWormEntityNPC.NPC.life != 0) //BossWormEntity still alive
                {
                    //kill the BossWormEntity
                    BossWormEntityNPC.NPC.life = 0;
                    BossWormEntityNPC.NPC.HitEffect(0, 10);
                    BossWormEntityNPC.NPC.checkDead();
                    BossWormEntityNPC.NPC.active = false;
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, BossWormEntityNPC.NPC.whoAmI, -1f);
                    return true;
                }
            }
            return false;
        }
    }

        /// <summary>
        /// The base class for head segment NPCs of Worm enemies
        /// </summary>
    public abstract class BossWormHead : BossWorm
    {
        public sealed override WormSegmentType SegmentType => WormSegmentType.Head;

        /// <summary>
        /// Whether the NPC ignores tile collision when attempting to "dig" through tiles, like how Wyverns work.
        /// </summary>
        public bool CanFly { get; set; }

        /// <summary>
        /// The maximum distance in <b>pixels</b> within which the NPC will use tile collision, if <see cref="CanFly"/> returns <see langword="false"/>.<br/>
        /// Defaults to 1000 pixels, which is equivalent to 62.5 tiles.
        /// </summary>
        public virtual int MaxDistanceForUsingTileCollision => 1000;

        /// <summary>
        /// If not <see langword="null"/>, this NPC will target the given world position instead of its player target
        /// </summary>
        public Vector2? ForcedTargetPosition { get; set; }

        internal sealed override void HeadAI()
        {
            CommonAI_Head(this);

            bool collision = HeadAI_CheckCollisionForDustSpawns();

            HeadAI_CheckTargetDistance(ref collision);

            HeadAI_Movement(collision);
        }

        internal void CommonAI_Head(BossWormHead worm)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if ((!worm.FollowerNPC.active)) // the segment after this head has died
                {
                    worm.NPC.life = 0; //kill this bodyless head
                    worm.NPC.HitEffect(0, 10);
                    worm.NPC.checkDead();
                    worm.NPC.active = false;
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, worm.NPC.whoAmI, -1f);
                    return;
                }
            }
        }

        private bool HeadAI_CheckCollisionForDustSpawns()
        {
            int minTilePosX = (int)(NPC.Left.X / 16) - 1;
            int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
            int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
            int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;

            // Ensure that the tile range is within the world bounds
            if (minTilePosX < 0)
                minTilePosX = 0;
            if (maxTilePosX > Main.maxTilesX)
                maxTilePosX = Main.maxTilesX;
            if (minTilePosY < 0)
                minTilePosY = 0;
            if (maxTilePosY > Main.maxTilesY)
                maxTilePosY = Main.maxTilesY;

            bool collision = false;

            // This is the initial check for collision with tiles.
            for (int i = minTilePosX; i < maxTilePosX; ++i)
            {
                for (int j = minTilePosY; j < maxTilePosY; ++j)
                {
                    Tile tile = Main.tile[i, j];

                    // If the tile is solid or is considered a platform, then there's valid collision
                    if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0) || tile.LiquidAmount > 64)
                    {
                        Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);

                        if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y && NPC.Top.Y < tileWorld.Y + 16)
                        {
                            // Collision found
                            collision = true;

                            if (Main.rand.NextBool(100))
                                WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
                        }
                    }
                }
            }

            return collision;
        }

        private void HeadAI_CheckTargetDistance(ref bool collision)
        {
            // If there is no collision with tiles, we check if the distance between this NPC and its target is too large, so that we can still trigger "collision".
            if (!collision)
            {
                Rectangle hitbox = NPC.Hitbox;

                int maxDistance = MaxDistanceForUsingTileCollision;

                bool tooFar = true;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Rectangle areaCheck;

                    Player player = Main.player[i];

                    if (ForcedTargetPosition is Vector2 target)
                        areaCheck = new Rectangle((int)target.X - maxDistance, (int)target.Y - maxDistance, maxDistance * 2, maxDistance * 2);
                    else if (player.active && !player.dead && !player.ghost)
                        areaCheck = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);
                    else
                        continue;  // Not a valid player

                    if (hitbox.Intersects(areaCheck))
                    {
                        tooFar = false;
                        break;
                    }
                }

                if (tooFar)
                    collision = true;
            }
        }

        private void HeadAI_Movement(bool collision)
        {
            // MoveSpeed determines the max speed at which this NPC can move.
            // Higher value = faster speed.
            float speed = MoveSpeed;
            // acceleration is exactly what it sounds like. The speed at which this NPC accelerates.
            float acceleration = Acceleration;

            float targetXPos, targetYPos;

            Player playerTarget = Main.player[NPC.target];

            Vector2 forcedTarget = ForcedTargetPosition ?? playerTarget.Center;
            // Using a ValueTuple like this allows for easy assignment of multiple values
            (targetXPos, targetYPos) = (forcedTarget.X, forcedTarget.Y);

            // Copy the value, since it will be clobbered later
            Vector2 npcCenter = NPC.Center;

            float targetRoundedPosX = (float)((int)(targetXPos / 16f) * 16);
            float targetRoundedPosY = (float)((int)(targetYPos / 16f) * 16);
            npcCenter.X = (float)((int)(npcCenter.X / 16f) * 16);
            npcCenter.Y = (float)((int)(npcCenter.Y / 16f) * 16);
            float dirX = targetRoundedPosX - npcCenter.X;
            float dirY = targetRoundedPosY - npcCenter.Y;

            float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

            // If we do not have any type of collision, we want the NPC to fall down and de-accelerate along the X axis.
            if (!collision && !CanFly)
                HeadAI_Movement_HandleFallingFromNoCollision(dirX, speed, acceleration);
            else
            {
                // Else we want to play some audio (soundDelay) and move towards our target.
                HeadAI_Movement_PlayDigSounds(length);

                HeadAI_Movement_HandleMovement(dirX, dirY, length, speed, acceleration);
            }

            HeadAI_Movement_SetRotation(collision);
        }

        private void HeadAI_Movement_HandleFallingFromNoCollision(float dirX, float speed, float acceleration)
        {
            // Keep searching for a new target
            NPC.TargetClosest(true);

            // Constant gravity of 0.11 pixels/tick
            NPC.velocity.Y += 0.11f;

            // Ensure that the NPC does not fall too quickly
            if (NPC.velocity.Y > speed)
                NPC.velocity.Y = speed;

            // The following behavior mimics vanilla worm movement
            if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4f)
            {
                // Velocity is sufficiently fast, but not too fast
                if (NPC.velocity.X < 0.0f)
                    NPC.velocity.X -= acceleration * 1.1f;
                else
                    NPC.velocity.X += acceleration * 1.1f;
            }
            else if (NPC.velocity.Y == speed)
            {
                // NPC has reached terminal velocity
                if (NPC.velocity.X < dirX)
                    NPC.velocity.X += acceleration;
                else if (NPC.velocity.X > dirX)
                    NPC.velocity.X -= acceleration;
            }
            else if (NPC.velocity.Y > 4)
            {
                if (NPC.velocity.X < 0)
                    NPC.velocity.X += acceleration * 0.9f;
                else
                    NPC.velocity.X -= acceleration * 0.9f;
            }
        }

        private void HeadAI_Movement_PlayDigSounds(float length)
        {
            if (NPC.soundDelay == 0)
            {
                // Play sounds quicker the closer the NPC is to the target location
                float num1 = length / 40f;

                if (num1 < 10)
                    num1 = 10f;

                if (num1 > 20)
                    num1 = 20f;

                NPC.soundDelay = (int)num1;

                SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
            }
        }

        private void HeadAI_Movement_HandleMovement(float dirX, float dirY, float length, float speed, float acceleration)
        {
            float absDirX = Math.Abs(dirX);
            float absDirY = Math.Abs(dirY);
            float newSpeed = speed / length;
            dirX *= newSpeed;
            dirY *= newSpeed;

            if ((NPC.velocity.X > 0 && dirX > 0) || (NPC.velocity.X < 0 && dirX < 0) || (NPC.velocity.Y > 0 && dirY > 0) || (NPC.velocity.Y < 0 && dirY < 0))
            {
                // The NPC is moving towards the target location
                if (NPC.velocity.X < dirX)
                    NPC.velocity.X += acceleration;
                else if (NPC.velocity.X > dirX)
                    NPC.velocity.X -= acceleration;

                if (NPC.velocity.Y < dirY)
                    NPC.velocity.Y += acceleration;
                else if (NPC.velocity.Y > dirY)
                    NPC.velocity.Y -= acceleration;

                // The intended Y-velocity is small AND the NPC is moving to the left and the target is to the right of the NPC or vice versa
                if (Math.Abs(dirY) < speed * 0.2 && ((NPC.velocity.X > 0 && dirX < 0) || (NPC.velocity.X < 0 && dirX > 0)))
                {
                    if (NPC.velocity.Y > 0)
                        NPC.velocity.Y += acceleration * 2f;
                    else
                        NPC.velocity.Y -= acceleration * 2f;
                }

                // The intended X-velocity is small AND the NPC is moving up/down and the target is below/above the NPC
                if (Math.Abs(dirX) < speed * 0.2 && ((NPC.velocity.Y > 0 && dirY < 0) || (NPC.velocity.Y < 0 && dirY > 0)))
                {
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
                    else
                        NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
                }
            }
            else if (absDirX > absDirY)
            {
                // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
                if (NPC.velocity.X < dirX)
                    NPC.velocity.X += acceleration * 1.1f;
                else if (NPC.velocity.X > dirX)
                    NPC.velocity.X -= acceleration * 1.1f;

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                {
                    if (NPC.velocity.Y > 0)
                        NPC.velocity.Y += acceleration;
                    else
                        NPC.velocity.Y -= acceleration;
                }
            }
            else
            {
                // The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
                if (NPC.velocity.Y < dirY)
                    NPC.velocity.Y += acceleration * 1.1f;
                else if (NPC.velocity.Y > dirY)
                    NPC.velocity.Y -= acceleration * 1.1f;

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                {
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X += acceleration;
                    else
                        NPC.velocity.X -= acceleration;
                }
            }
        }

        private void HeadAI_Movement_SetRotation(bool collision)
        {
            // Set the correct rotation for this NPC.
            // Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

            // Some netupdate stuff (multiplayer compatibility).
            if (collision)
            {
                if (NPC.localAI[0] != 1)
                    NPC.netUpdate = true;

                NPC.localAI[0] = 1f;
            }
            else
            {
                if (NPC.localAI[0] != 0)
                    NPC.netUpdate = true;

                NPC.localAI[0] = 0f;
            }

            // Force a netupdate if the NPC's velocity changed sign and it was not "just hit" by a player
            if (((NPC.velocity.X > 0 && NPC.oldVelocity.X < 0) || (NPC.velocity.X < 0 && NPC.oldVelocity.X > 0) || (NPC.velocity.Y > 0 && NPC.oldVelocity.Y < 0) || (NPC.velocity.Y < 0 && NPC.oldVelocity.Y > 0)) && !NPC.justHit)
                NPC.netUpdate = true;
        }

        /// <summary>
        /// This method caculates and returns the worm bosses actual and total health (only possible for the original head)
        /// </summary>
    }

    public abstract class BossWormBody : BossWorm
    {
        public sealed override WormSegmentType SegmentType => WormSegmentType.Body;
        internal override void BodyAI()
        {
            CommonAI_Body(this);
        }

        public void CommonAI_Body(BossWorm worm)
        {
            if (!worm.NPC.HasValidTarget)
                worm.NPC.TargetClosest(true);

            if (Main.player[worm.NPC.target].dead && worm.NPC.timeLeft > 30000)
                worm.NPC.timeLeft = 10;

            NPC following = worm.NPC.ai[1] >= Main.maxNPCs ? null : worm.FollowingNPC;
             if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if ( (!worm.FollowingNPC.active) || (worm.FollowingNPC.aiStyle != worm.NPC.aiStyle) ) // the segment in front of this body has died or has no "body-segment-AI"
                {
                    int changeType = ((worm.NPC.ModNPC as BossWorm).BossWormEntityNPC as BossWormEntity).HeadType;
                    int temp38 = worm.NPC.whoAmI;
                    float temp39 = (float)worm.NPC.life / (float)worm.NPC.lifeMax;
                    float temp40 = worm.NPC.ai[0];
                    BossWormEntity temp41 = worm.BossWormEntityNPC;

                    worm.NPC.SetDefaultsKeepPlayerInteraction(changeType); //change this segment into a boss worm head

                    worm.NPC.life = (int)((float)worm.NPC.lifeMax * temp39);
                    worm.NPC.ai[0] = temp40;
                    worm.NPC.TargetClosest();
                    worm.NPC.netUpdate = true;
                    worm.NPC.whoAmI = temp38;
                    worm.NPC.alpha = 0;
                    (worm.NPC.ModNPC as BossWorm).BossWormEntityNPC = temp41;
                    return;
                }

                if ((!worm.FollowerNPC.active) || (worm.FollowerNPC.aiStyle != worm.NPC.aiStyle)) // the segment after this body has died or has no "body-segment-AI"
                {
                    int changeType = ((worm.NPC.ModNPC as BossWorm).BossWormEntityNPC as BossWormEntity).TailType;
                    int temp38 = worm.NPC.whoAmI;
                    float temp39 = (float)worm.NPC.life / (float)worm.NPC.lifeMax;
                    float temp40 = worm.NPC.ai[1];
                    BossWormEntity temp41 = BossWormEntityNPC;

                    worm.NPC.SetDefaultsKeepPlayerInteraction(changeType); //change this segment into a boss worm tail

                    worm.NPC.life = (int)((float)worm.NPC.lifeMax * temp39);
                    worm.NPC.ai[1] = temp40;
                    worm.NPC.TargetClosest();
                    worm.NPC.netUpdate = true;
                    worm.NPC.whoAmI = temp38;
                    worm.NPC.alpha = 0;
                    (worm.NPC.ModNPC as BossWorm).BossWormEntityNPC = temp41;
                    return;
                }

                if ((!worm.FollowerNPC.active) && (!worm.FollowingNPC.active)) // the segment after AND in front of this body have died
                {
                    worm.NPC.life = 0; //kill this connectionless body
                    worm.NPC.HitEffect(0, 10);
                    worm.NPC.checkDead();
                    worm.NPC.active = false;
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, worm.NPC.whoAmI, -1f);

                    return;
                }
                //// Some of these conditions are possible if the body/tail segment was spawned individually
                //// Kill the segment if the segment NPC it's following is no longer valid
                //if (following is null || !following.active || following.friendly || following.townNPC || following.lifeMax <= 5)
                //{
                //    worm.NPC.life = 0;
                //    worm.NPC.HitEffect(0, 10);
                //    worm.NPC.active = false;
                //}
            }

            if (following is not null)
            {
                // Follow behind the segment "in front" of this NPC
                // Use the current NPC.Center to calculate the direction towards the "parent NPC" of this NPC.
                float dirX = following.Center.X - worm.NPC.Center.X;
                float dirY = following.Center.Y - worm.NPC.Center.Y;
                // We then use Atan2 to get a correct rotation towards that parent NPC.
                // Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
                worm.NPC.rotation = (float)Math.Atan2(dirY, dirX) + MathHelper.PiOver2;
                // We also get the length of the direction vector.
                float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                // We calculate a new, correct distance.
                float dist = (length - worm.NPC.width) / length;
                float posX = dirX * dist;
                float posY = dirY * dist;

                // Reset the velocity of this NPC, because we don't want it to move on its own
                worm.NPC.velocity = Vector2.Zero;
                // And set this NPCs position accordingly to that of this NPCs parent NPC.
                worm.NPC.position.X += posX;
                worm.NPC.position.Y += posY;
            }
        }
    }

    // Since the body and tail segments share the same AI
    public abstract class BossWormTail : BossWorm
    {
        public sealed override WormSegmentType SegmentType => WormSegmentType.Tail;

        internal override void TailAI()
        {
            CommonAI_Tail(this);
        }
        internal void CommonAI_Tail(BossWormTail worm)
        {
            if (!worm.NPC.HasValidTarget)
                worm.NPC.TargetClosest(true);

            if (Main.player[worm.NPC.target].dead && worm.NPC.timeLeft > 30000)
                worm.NPC.timeLeft = 10;

            NPC following = worm.NPC.ai[1] >= Main.maxNPCs ? null : worm.FollowingNPC;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if ((!worm.FollowingNPC.active) ) // the segment in front of this tail has died
                {
                    worm.NPC.life = 0; //kill this headless tail
                    worm.NPC.HitEffect(0, 10);
                    worm.NPC.checkDead();
                    worm.NPC.active = false;
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, worm.NPC.whoAmI, -1f);
                    return;
                }
            }

            if (following is not null)
            {
                // Follow behind the segment "in front" of this NPC
                // Use the current NPC.Center to calculate the direction towards the "parent NPC" of this NPC.
                float dirX = following.Center.X - worm.NPC.Center.X;
                float dirY = following.Center.Y - worm.NPC.Center.Y;
                // We then use Atan2 to get a correct rotation towards that parent NPC.
                // Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
                worm.NPC.rotation = (float)Math.Atan2(dirY, dirX) + MathHelper.PiOver2;
                // We also get the length of the direction vector.
                float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                // We calculate a new, correct distance.
                float dist = (length - worm.NPC.width) / length;
                float posX = dirX * dist;
                float posY = dirY * dist;

                // Reset the velocity of this NPC, because we don't want it to move on its own
                worm.NPC.velocity = Vector2.Zero;
                // And set this NPCs position accordingly to that of this NPCs parent NPC.
                worm.NPC.position.X += posX;
                worm.NPC.position.Y += posY;
            }
        }
    }
}
