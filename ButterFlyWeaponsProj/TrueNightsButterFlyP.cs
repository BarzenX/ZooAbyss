﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.DamageClasses;

namespace ZooAbyss.ButterFlyWeaponsProj
{
    public class TrueNightsButterFlyP : ModProjectile
    {
        public int ProjDelay = 10; // frames remaining till we can fire a projectile again
        public int ProjDamage = 50; // frames remaining till we can fire a projectile again
        public override void SetDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<CaretakerFlutterDamageClass>();
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 3;
            Projectile.timeLeft = 290;
            AIType = 52;
            Projectile.damage = 70;

        }

        public override void AI()
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.TerraBlade, 0f, 0f, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.3f;
            Main.dust[dust].scale = Main.rand.Next(50, 135) * 0.012f;

            int dust2 = Dust.NewDust(Projectile.Center, 1, 1, DustID.GemSapphire, 0f, 0f, 0, default, 1f);
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].velocity *= 0.3f;
            Main.dust[dust2].scale = Main.rand.Next(50, 135) * 0.012f;


            Player owner = Main.player[Projectile.owner]; // Get the owner of the projectile.
            if (Main.myPlayer == Projectile.owner)
            {
                if (ProjDelay > 0)
                {
                    ProjDelay--;
                }
                if (ProjDelay <= 5)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(null), Projectile.Center, Projectile.velocity * +3, ProjectileID.NightBeam, ProjDamage, 1f, Main.myPlayer, -1, -1);
                    ProjDelay = 20;

                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 300);
        }
    }
}













