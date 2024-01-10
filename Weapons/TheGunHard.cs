using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ZooAbyss.Ammo;
using ZooAbyss.projectiles;

namespace ZooAbyss.Weapons
{
    public class TheGunHard : ModItem
    {
        public override void SetDefaults()
        {
            // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.

            // Common Properties
            Item.width = 62; // Hitbox width of the item.
            Item.height = 32; // Hitbox height of the item.
            Item.scale = 0.75f;
            Item.rare = ItemRarityID.Blue; // The color that the item's name will be in-game.
            Item.value = 1000;

            // Weapon Properties
            Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
            Item.damage = 20; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
            Item.knockBack = 2f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
            Item.noMelee = true; // So the item's animation doesn't do damage.

            Item.useTime = 25; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 25; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = false; // Whether or not you can hold click to automatically use it again.

            //Item.UseSound = SoundID.Item11;
            Item.UseSound = new SoundStyle($"{nameof(ZooAbyss)}/Sound/TheGunPreFireSound")
            {
                Volume = 0.9f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };

            // Gun Properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this....must be 10 by convention
            Item.shootSpeed = 20f; // The speed of the projectile (measured in pixels per frame.)
            Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.

        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, -1f);
        }
        
        
          
        


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<TheGunPre>(), 1);
            recipe.AddIngredient(ItemID.SquirrelCage, 1);
            recipe.AddIngredient(ItemID.WaterStrider, 1);
            recipe.AddIngredient(ItemID.RifleScope, 1);
            recipe.AddIngredient(ItemID.Pearlwood, 30);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.Register();
        }
    }   
    
}