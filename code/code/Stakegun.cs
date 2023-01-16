using Sandbox;
using Editor;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkweapons;

[Library("pk_stakegun"), HammerEntity]
[EditorModel("models/weapons/painkiller/w_stakegun.vmdl")]
[Title("Painkiller: Stakegun")]
[Spawnable]
public partial class PKStakegun : PKWepBase
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_stakegun.vmdl");
    public override string ViewModelPath => "models/weapons/painkiller/v_stakegun.vmdl";
    public override float PrimaryRate => 0.65f;
    public override float SecondaryRate => 1.75f;

    public override void Spawn()
    {
        base.Spawn();
        Model = WorldModel;
    }
    public override void Simulate(IClient owner)
    {
        if (Owner != null)
        {
            if (!CanPrimaryAttack() && !CanSecondaryAttack())
                return;

            if (Input.Pressed(InputButton.PrimaryAttack) && Input.Pressed(InputButton.SecondaryAttack))
                DoCombo();
        }

        base.Simulate(owner);
    }

    public override void AttackPrimary()
    {
        TimeSincePrimaryAttack = 0;
        TimeSinceSecondaryAttack = -1.5f;

        ShootEffects();
        PlaySound("stakegun_fire_primary");
        (Owner as AnimatedEntity).SetAnimParameter("b_fire", true);

        if (Game.IsServer)
        {
            var stake = new Stake();
            stake.Position = Owner.AimRay.Position;
            stake.Rotation = Owner.Rotation;
            stake.Owner = Owner;
            stake.Velocity = Owner.AimRay.Forward * 50;
        }
    }

    [ClientRpc]
    public override void DryFire()
    {
        PlaySound("shotgun_dryfire");
    }

    public override void DoCombo()
    {
        TimeSincePrimaryAttack = 0.0f;
        TimeSinceSecondaryAttack = -0.25f;

        PlaySound("stakegun_combo");
        ShootEffects();

        if (Game.IsServer)
        {
            var stake = new Stake();
            stake.Position = Owner.AimRay.Position;
            stake.Rotation = Owner.Rotation;
            stake.Owner = Owner;
            stake.Velocity = Owner.AimRay.Forward * 50;

            var grenade = new StakeGrenade
            {
                Position = Owner.AimRay.Position + Owner.AimRay.Forward * 5.0f,
                Rotation = Owner.Rotation,
                Owner = Owner
            };

            grenade.SetParent(stake);
            grenade.IsParented = true;
            stake.Grenade = grenade;
        }

    }

    public override void AttackSecondary()
    {
        TimeSincePrimaryAttack = 0.90f;
        TimeSinceSecondaryAttack = -0.25f;

        ShootSecondaryEffects();

        PlaySound( "stakegun_fire_secondary" );

        FireGrenade(Owner.AimRay.Position, Owner.AimRay.Forward, 0.0f, 1.0f, 2.0f, 2.0f);
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

        ViewModelEntity?.SetAnimParameter("b_primary_fire", true);
    }

    public void FireGrenade(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize)
    {
        if (!Game.IsServer)
            return;

        PlaySound("dm.grenade_throw");

        Game.SetRandomSeed(Time.Tick);

        if (Game.IsServer)
            using (Prediction.Off())
            {
                var grenade = new StakeGrenade
                {
                    Position = Owner.AimRay.Position + Owner.AimRay.Forward * 5.0f,
                    Rotation = Owner.Rotation,
                    Owner = Owner
                };

                grenade.PhysicsBody.Velocity = Owner.AimRay.Forward * 600.0f + Owner.Rotation.Up * 200.0f + Owner.Velocity;

                _ = grenade.BlowIn(3.0f);
            }

        (Owner as AnimatedEntity).SetAnimParameter("b_attack", true);
    }
}
