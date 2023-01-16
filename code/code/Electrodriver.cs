using Sandbox;
using Editor;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pkweapons;

[Library("pk_electrodriver"), HammerEntity]
[EditorModel("models/weapons/painkiller/w_electrodriver.vmdl")]
[Title("Painkiller: Electodriver")]
[Spawnable]
public partial class PKElectroDriver : PKWepBase
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_electrodriver.vmdl");
    public override string ViewModelPath => "models/weapons/painkiller/v_electrodriver.vmdl";
    public override float PrimaryRate => 5.0f;
    public override float SecondaryRate => 4.5f;

    bool secondLooping;
    Entity lockedTarget;
    Sound loopSound;

    public override void Spawn()
    {
        base.Spawn();
        Model = WorldModel;
    }

    public override void Simulate(IClient owner)
    {
        base.Simulate(owner);

        if (Owner != null)
        {
            if (Owner != null)
            {
                if (Input.Pressed(InputButton.PrimaryAttack) && Input.Down(InputButton.SecondaryAttack))
                    DoCombo();
            }

            secondLooping = Input.Down(InputButton.SecondaryAttack);

            if(!secondLooping && !loopSound.Finished)
            {
                PlaySound("electro_stop");
                loopSound.Stop();
            }
        }
    }

    public override void AttackPrimary()
    {
        TimeSincePrimaryAttack = 0.0f;
        TimeSinceSecondaryAttack = 0.0f;

        ShootEffects();
        PlaySound("electrodriver_primfire");
        (Owner as AnimatedEntity).SetAnimParameter("b_fire", true);

        if (Game.IsServer)
        {
            var shuriken = new Shuriken();
            shuriken.Position = Owner.AimRay.Position;
            shuriken.Rotation = Owner.Rotation;
            shuriken.Owner = Owner;
            shuriken.Velocity = Owner.AimRay.Forward * 50;
        }
    }

    [ClientRpc]
    public override void DryFire()
    {
        PlaySound("shotgun_dryfire");
    }

    public override void DoCombo()
    {
        TimeSincePrimaryAttack = -1.0f;
        TimeSinceSecondaryAttack = -1.0f;

        ComboEffects();
        PlaySound("electro_combo");

        if (Game.IsServer)
        {
            var disk = new ElectroDisk();
            disk.Position = Owner.AimRay.Position;
            disk.Rotation = Owner.Rotation;
            disk.Owner = Owner;
            disk.Velocity = Owner.AimRay.Forward * 50;
        }
    }

    [ClientRpc]
    public override void ComboEffects()
    {
        Game.AssertClient();

        ViewModelEntity?.SetAnimParameter("b_combo", true);
    }

    Entity ScanForTarget()
    {
        float range = 250.0f;

        var trLine = Trace.Ray(Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * range)
            .Ignore(Owner)
            .Run();

        var trSphere = Trace.Sphere(64.0f, Owner.AimRay.Position, Owner.AimRay.Position + Owner.Rotation.Forward * range)
            .Ignore(Owner)
            .Run();

        return trLine.Entity?? trSphere.Entity;
    }

    public override void AttackSecondary()
    {
        TimeSincePrimaryAttack = 0.0f;

        ShootSecondaryEffects();

        if (!Game.IsServer)
            return;

        if (!secondLooping)
            PlaySound( "electro_start" );
        else
        {
            if (loopSound.Finished)
                loopSound = PlaySound("electro_loop");

            if (lockedTarget == null)
                lockedTarget = ScanForTarget();
            else
            {
                DamageInfo dmgInfo = new DamageInfo();
                dmgInfo.Damage = 15.0f;
                dmgInfo.Attacker = Owner;
                dmgInfo.WithWeapon(this);

                lockedTarget.TakeDamage(dmgInfo);

                if(lockedTarget.Position.Distance(Owner.Position) > 300.0f)
                    lockedTarget = null;

                if (lockedTarget != null && lockedTarget.LifeState == LifeState.Dead)
                    lockedTarget = null;

                return;
            }
        }
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
}
