using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using ZooAbyss.Items;


namespace ZooAbyss.NPCs
{
    public class GreenBeetle : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 30;
            NPC.scale = 0.25f;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 10;
            NPC.value = 0f;
            NPC.chaseable = false;
            NPC.npcSlots = 0.5f;
            NPC.aiStyle = 67;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            AIType = NPCID.Buggy;
            AnimationType = NPCID.Buggy;
            Main.npcCatchable[NPC.type] = true;
            NPC.catchItem = ModContent.ItemType<GreenBeetleItem>();




        }

        public override void AI()
        {
            NPC.scale = 1f;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.OverworldDay.Chance * 0.5f;
        }
    }
}