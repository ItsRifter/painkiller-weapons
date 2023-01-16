using Sandbox;
using Editor;
using System.Collections.Generic;
using System.Linq;

namespace pkweapons;

[Library("pk_shotgun"), HammerEntity]
[EditorModel("models/weapons/painkiller/w_shotgun.vmdl")]
[Title("Painkiller: Shotgun")]
[Spawnable]
public partial class PKShotgun : PKWepBase
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_shotgun.vmdl");
    public override string ViewModelPath => "models/weapons/painkiller/v_shotgun.vmdl";
    public override float PrimaryRate => 1.5f;
    public override float SecondaryRate => 1;

    Dictionary<AnimatedEntity, TimeSince> frozenPlayers;

    public override void Spawn()
    {
        base.Spawn();
        frozenPlayers = new Dictionary<AnimatedEntity, TimeSince>();
        Model = WorldModel;
    }

    public override void Simulate(IClient owner)
    {
        base.Simulate(owner);

        if (frozenPlayers != null && frozenPlayers.Count > 0)
        {
            foreach (var player in frozenPlayers.ToArray())
            {
                if(player.Key == null)
                    frozenPlayers.Remove(player.Key);

                if(player.Key.LifeState == LifeState.Dead)
                {
                    player.Key.RenderColor = Color.White;
                    player.Key.PlaySound("freezer_unfreeze");
                    frozenPlayers.Remove(player.Key);
                }

                if(player.Value >= 7.5f)
                {
                    player.Key.RenderColor = Color.White;
                    player.Key.PlaySound("freezer_unfreeze");
                    frozenPlayers.Remove(player.Key);
                } 
                else
                    player.Key.Velocity = Vector3.Zero;
            }
        }
    }

    public override void AttackPrimary()
    {
        TimeSincePrimaryAttack = 0;
        TimeSinceSecondaryAttack = 0;

        ShootEffects();
        PlaySound("shotgun_fire_primary");

        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);

        ShootBullets(8, 0.45f, 0.1f, 5.0f, 2.0f);
    }

    [ClientRpc]
    public override void DryFire()
    {
        PlaySound("shotgun_dryfire");
    }

    public override void AttackSecondary()
    {
        TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

        ShootSecondaryEffects();

        PlaySound( "freezer_shoot" );

        ShootFreezer(Owner.AimRay.Position, Owner.Rotation.Forward, 0.0f, 1.0f, 2.0f, 2.0f);
    }

    [ClientRpc]
    protected void ShootSecondaryEffects()
    {
        Game.AssertClient();

        ViewModelEntity?.SetAnimParameter("b_second_fire", true);
    }

    [ClientRpc]
    protected override void ShootEffects()
    {
        Game.AssertClient();

        Particles.Create("particles/shotgun_muzzleflash.vpcf", EffectEntity, "muzzle");
        Particles.Create("particles/shotgun_muzzleflash.vpcf", EffectEntity, "muzzle1");

        ViewModelEntity?.SetAnimParameter("b_primary_fire", true);
    }

    public void ShootFreezer(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize)
    {
        if (!Game.IsServer)
            return;

        var forward = dir;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;

        //var tr = TraceBullet(pos, pos + forward * 5000, bulletSize);

        var trace = Trace.Ray(pos, pos + forward * 5000)
                 .UseHitboxes()
                 .WithAnyTags("solid", "player", "npc", "glass")
                 .Ignore(Owner)
                 .Size(bulletSize)
                 .Run();

        if (trace.Entity == null)
            return;

        using (Prediction.Off())
        {
            var damageInfo = DamageInfo.FromBullet(trace.EndPosition, forward * 100 * force, damage)
                .UsingTraceResult(trace)
                .WithAttacker(Owner)
                .WithWeapon(this);

            trace.Entity.TakeDamage(damageInfo);

            if(trace.Entity is AnimatedEntity pawn)
            {
                if (frozenPlayers.ContainsKey(pawn))
                    return;

                pawn.PlaySound("freezer_freeze");
                pawn.RenderColor = Color.Blue;
                frozenPlayers.Add(pawn, 0.0f);
            }
        }
    }
}
