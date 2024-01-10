using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.ButterFlyWeaponsProj;
using ZooAbyss.DamageClasses;

namespace ZooAbyss.ButterFlyWeapons
{
    public class BoneButterFly : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 35;
            Item.knockBack = 2;
            Item.noUseGraphic = true;
            Item.useTime = 10;
            Item.autoReuse = false;
            Item.UseSound = SoundID.Item1;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1.5f;
            Item.height = 50;
            Item.rare = ItemRarityID.Gray;
            Item.value = Item.buyPrice(0, 2, 0, 0);
            Item.shoot = ModContent.ProjectileType<BoneButterFlyP>();
            Item.shootSpeed = 15;
            Item.DamageType = ModContent.GetInstance<CaretakerFlutterDamageClass>();
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
        
    }
}