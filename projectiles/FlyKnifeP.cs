﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ZooAbyss.projectiles
{
    public class FlyKnifeP : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = - 1;
            Projectile.aiStyle = 2;
            Projectile.timeLeft = 300;
            AIType = 23;
            Projectile.damage = 30;
           
        }

        public override void AI()
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Snow, 0f, 0f, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.3f;
            Main.dust[dust].scale = (float)Main.rand.Next(100, 135) * 0.010f;

            int dust2 = Dust.NewDust(Projectile.Center, 1, 1, DustID.Ebonwood, 0f, 0f, 0, default, 1f);
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].velocity *= 0.3f;
            Main.dust[dust2].scale = (float)Main.rand.Next(50, 135) * 0.013f;
        }
    } 
}
