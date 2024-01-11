using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;
using ZooAbyss.Bosses;

namespace ZooAbyss.Bosses.BossLifeBar
{
    public class MotherWurmBossBar : ModBossBar
    {
        // Showcases a custom boss bar with basic logic for displaying the icon, life, and shields properly.
        // Has no custom texture, meaning it will use the default vanilla boss bar texture

        private int bossHeadIndex = -1; //init value

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            if (bossHeadIndex != -1) // init value was overwritten
            {
                return TextureAssets.NpcHeadBoss[bossHeadIndex];
            }
            return null;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            // Here the game wants to know if to draw the boss bar or not. Return false whenever the conditions don't apply.
            // If there is no possibility of returning false (or null) the bar will get drawn at times when it shouldn't, so write defensive code!

            NPC bossBarNPC = Main.npc[info.npcIndexToAimAt];
            if ((!bossBarNPC.active) || // the BossWormEntity has gone
               ( (bossBarNPC.ModNPC as BossWormEntity).startDespawning))
            {
                return false; // boss was slain or despawned, hide bossbar
            }

            // We assign bossHeadIndex here because we need to use it in GetIconTexture
            if (bossHeadIndex == -1)    bossHeadIndex = bossBarNPC.GetBossHeadTextureIndex();
            
            life = (bossBarNPC.ModNPC as BossWorm).BossWormEntityNPC.TotalCurrentLife;
            lifeMax = (bossBarNPC.ModNPC as BossWorm).BossWormEntityNPC.TotalMaxLife;
            //if (bossBarNPC.ModNPC is not MotherWurmHead myModNPC) return null; //TODO: False
            //life = (float)myModNPC.TotalCurrentLife;
            //lifeMax = (float)myModNPC.TotalMaxLife;
            shield = 0f;
            shieldMax = 0f;

            return true;
        }
    }
}