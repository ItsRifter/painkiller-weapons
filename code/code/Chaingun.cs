using Sandbox;
using Editor;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkweapons;

[Library("pk_chaingun"), HammerEntity]
[EditorModel("models/weapons/painkiller/w_chaingun.vmdl")]
[Title("Painkiller: Chaingun")]
[Spawnable]
public partial class PKChaingun : PKWepBase
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_chaingun.vmdl");
    public override string ViewModelPath => "models/weapons/painkiller/v_chaingun.vmdl";
    public override float PrimaryRate => 0.85f;
    public override float SecondaryRate => 12.5f;

    Sound motorSound;
    Sound loopSound;
    TimeSince timeLoop;

    public override void Spawn()
    {
        base.Spawn();
        Model = WorldModel;
    }
    public override void Simulate(IClient owner)
    {
        base.Simulate(owner);

        if (!Input.Down(InputButton.SecondaryAttack) && (!motorSound.Finished || !loopSound.Finished))
        {
            PlaySound("chaingun_shoot_secondend");
            motorSound.Stop();
            loopSound.Stop();
        } 
        
        ShootSecondaryEffects();
    }

    public override void AttackPrimary()
    {
        TimeSincePrimaryAttack = 0.0f;
        TimeSinceSecondaryAttack = 0.0f;

        ShootEffects();

        PlaySound("chaingun_shoot_primary");

        (Owner as AnimatedEntity).SetAnimParameter("b_fire", true);

        if (Game.IsServer)
        {
            var bolt = new RocketGrenade();
            bolt.Position = Owner.AimRay.Position + Owner.AimRay.Forward * 100;
            bolt.Rotation = Owner.Rotation;
            bolt.Owner = Owner;
            bolt.Velocity = Owner.AimRay.Forward * 50;
        }
    }

    [ClientRpc]
    public override void DryFire()
    {
        PlaySound("shotgun_dryfire");
    }

    public override void AttackSecondary()
    {
        TimeSincePrimaryAttack = -0.25f;
        TimeSinceSecondaryAttack = 0.0f;

        if (loopSound.Finished)
            loopSound = PlaySound("chaingun_shoot_loop");

        if (motorSound.Finished)
            motorSound = PlaySound("chaingun_motor");

        if(timeLoop >= 0.05f)
            timeLoop = 0.0f;

        //ShootSecondaryEffects();
        ShootBullet(Owner.AimRay.Position, Owner.AimRay.Forward, 0.0f, 1.0f, 5.0f, 2.0f);
    }

    [ClientRpc]
    protected void ShootSecondaryEffects()
    {
        Game.AssertClient();

        if (timeLoop < 0.01f)
        {
            ViewModelEntity?.SetAnimParameter("b_second_fire", true);
            //Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle");
        }
    }

    [ClientRpc]
    protected override void ShootEffects()
    {
        Game.AssertClient();

        ViewModelEntity?.SetAnimParameter("b_primary_fire", true);
    }
}
