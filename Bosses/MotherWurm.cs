using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Bosses.BossLifeBar;
using Terraria.GameContent.ItemDropRules;
using ZooAbyss.Bosses.BossItems;
using ZooAbyss.Bosses;

namespace ZooAbyss.Bosses
{
    // These four class showcase usage of the Entity, Head, Body and Tail classes from BossWorm.cs

    [AutoloadBossHead] // This attribute looks for a texture called "ClassName_Head_Boss" and automatically registers it as the NPC boss head icon
    internal class MotherWurmEntity : BossWormEntity
    {
        public override int HeadType => ModContent.NPCType<MotherWurmHead>();
        public override int BodyType => ModContent.NPCType<MotherWurmBody>();
        public override int TailType => ModContent.NPCType<MotherWurmTail>();
        public override void Init()
        {
            // Set the segment variance
            // If you want the segment length to be constant, set these two properties to the same value
            MinSegmentLength = 100;
            MaxSegmentLength = 120; //keep this below 200 (maximum simultaneous monster count on screen possible)
        }

        public override void SetStaticDefaults()
        {
            /*var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            { // Influences how the NPC looks in the Bestiary
                CustomTexturePath = "ExampleMod/Content/NPCs/ExampleWorm_Bestiary", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);*/
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerHead);
            NPC.BossBar = ModContent.GetInstance<MotherWurmBossBar>();
            NPC.aiStyle = -1; //-1 = custom ai (entity wil always be teleported to a tail
            NPC.lifeMax = 100000; //the entity shall not be killed by normal means
            NPC.defense = 200000; //the entity shall not be killed by normal means
            NPC.dontTakeDamage = true; //the entity shall not be killed by normal means
            NPC.damage = 0; //the entity shall not do damage
            NPC.boss = true;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Sound/Music/MotherWurmBossMusic");
            }

        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Do NOT misuse the ModifyNPCLoot and OnKill hooks: the former is only used for registering drops, the latter for everything else

            // Add the treasure bag using ItemDropRule.BossBag (automatically checks for expert mode)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MotherWormBossBag>()));



        }
        public override bool CheckActive()
        {
            return this.startDespawning;
        }
    }
    internal class MotherWurmHead : BossWormHead
    {
        public override void SetStaticDefaults()
        {
            /*var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            { // Influences how the NPC looks in the Bestiary
                CustomTexturePath = "ExampleMod/Content/NPCs/ExampleWorm_Bestiary", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);*/
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerHead);
            NPC.aiStyle = 6; //6 = Worm AI
            NPC.lifeMax = 1000; //life needs to be the same for every worm segment or the total boss health will jump when a segment dies
            NPC.defense = 20; //adjust the "how hard to kill" for each segment with the defense or ignoring damage
            NPC.damage = 50;

        }



        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("MAMA.")
            });
        }

        public override void Init()
        {
            CommonWormInit(this);
        }

        // This method is invoked from ExampleWormHead, ExampleWormBody and ExampleWormTail
        internal static void CommonWormInit(BossWorm worm)
        {
            // These two properties handle the movement of the worm
            worm.MoveSpeed = 40f;
            worm.Acceleration = 0.2f;
        }

        private int attackCounter;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackCounter);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackCounter = reader.ReadInt32();
        }

        public override bool CheckActive()
        {
            return this.startDespawning;
        }

    }


    internal class MotherWurmBody : BossWormBody
    {
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerBody);
            NPC.aiStyle = 6; // 6=Worm AI
            NPC.lifeMax = 1000; //life needs to be the same for every worm segment or the total boss health will jump when a segment dies
            NPC.defense = 25; //adjust the "how hard to kill" for each segment with the defense or ignoring damage
            NPC.damage = 40;
        }

        public override void Init()
        {
            MotherWurmHead.CommonWormInit(this);
        }

        public override bool CheckActive()
        {
            return this.startDespawning;
        }
    }


    internal class MotherWurmTail : BossWormTail
    {
        public override void SetStaticDefaults()
        {
            /*NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);*/
            //TODO: Bestiary?
        }

        public override void SetDefaults() 
        {
            NPC.CloneDefaults(NPCID.DiggerTail);
            NPC.aiStyle = 6; //6 = Worm AI
            NPC.lifeMax = 1000; //life needs to be the same for every worm segment or the total boss health will jump when a segment dies
            NPC.defense = 35; //adjust the "how hard to kill" for each segment with the defense or ignoring damage
            NPC.damage = 40;
        }

        public override void Init()
        {
            MotherWurmHead.CommonWormInit(this);
        }

        public override bool CheckActive()
        {
            return this.startDespawning;
        }
    }
}