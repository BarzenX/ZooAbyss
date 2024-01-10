﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.projectiles;

namespace ZooAbyss.Weapons
{
    public class TerraFormer : ModItem   
    {
        public override void SetDefaults()
        {
            Item.damage = 160;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = false;
            Item.knockBack = 2;
            Item.value = 100000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TerraFormerP>();
            Item.shootSpeed = 20;
            Item.channel = true;
            Item.width = 72;
            Item.height = 60;

        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, -2f);
        }

    }
}