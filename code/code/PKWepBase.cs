using Sandbox;
using System;
using System.Collections.Generic;

namespace pkweapons;

public partial class PKWepBase : BaseCarriable, IUse
{
    public GrabTrigger PickupTrigger { get;  set; }

    [Net, Predicted]
    public TimeSince TimeSinceReload { get; set; }

    [Net, Predicted]
    public bool IsReloading { get; set; }

    [Net, Predicted]
    public TimeSince TimeSinceDeployed { get; set; }

    [Net, Predicted]
    public int PrimaryAmmoClip { get; set; }

    [Net, Predicted]
    public int SecondaryAmmoClip { get; set; }

    public virtual float PrimaryRate => 5f;

    public virtual float SecondaryRate => 15f;

    [Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
    [Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

    /* [Net, Predicted]
     public TimeSince TimeSincePrimaryAttack
     {
         get
         {
             return _repback__TimeSincePrimaryAttack.GetValue();
         }
         set
         {
             _repback__TimeSincePrimaryAttack.SetValue(in value);
         }
     }

     [Net, Predicted]
     public TimeSince TimeSinceSecondaryAttack
     {
         get
         {
             return _repback__TimeSinceSecondaryAttack.GetValue();
         }
         set
         {
             _repback__TimeSinceSecondaryAttack.SetValue(in value);
         }
     }*/
    public override void Spawn()
    {
        base.Spawn();

        PickupTrigger = new GrabTrigger
        {
            Parent = this,
            Position = Position,
            EnableTouch = true,
            EnableSelfCollisions = false
        };

        PickupTrigger.PhysicsBody.AutoSleep = false;
    }

    public override void ActiveStart(Entity ent)
    {
        base.ActiveStart(ent);

        TimeSinceDeployed = 0;
    }

    public virtual bool CanPrimaryAttack()
    {
        if (!Owner.IsValid() || !Input.Down(InputButton.PrimaryAttack))
        {
            return false;
        }

        float primaryRate = PrimaryRate;
        if (primaryRate <= 0f)
        {
            return true;
        }

        return (float)TimeSincePrimaryAttack > 1f / primaryRate;
    }

    public virtual bool CanSecondaryAttack()
    {
        if (!Owner.IsValid() || !Input.Down(InputButton.SecondaryAttack))
        {
            return false;
        }

        float secondaryRate = SecondaryRate;
        if (secondaryRate <= 0f)
        {
            return true;
        }

        return (float)TimeSinceSecondaryAttack > 1f / secondaryRate;
    }

    public virtual void AttackPrimary() { }
    public virtual void AttackSecondary() { }

    public override void Simulate(IClient owner)
    {
        base.Simulate(owner);
    }

    public virtual void DoCombo()
    {
        
    }

    public override void CreateViewModel()
    {
        Game.AssertClient();

        if (string.IsNullOrEmpty(ViewModelPath))
            return;

        ViewModelEntity = new BaseVM
        {
            Position = Position,
            Owner = Owner,
            EnableViewmodelRendering = true
        };

        ViewModelEntity.SetModel(ViewModelPath);
    }

    public bool OnUse(Entity user)
    {
        if (Owner != null)
            return false;

        if (!user.IsValid())
            return false;

        user.StartTouch(this);

        return false;
    }

    public bool TakeAmmo(int amount, bool isSecondary = false)
    {
        if (PrimaryAmmoClip < amount)
            return false;

        PrimaryAmmoClip -= amount;
        return true;
    }

    public virtual bool IsUsable(Entity user)
    {
        if (Owner == null) return false;

        return true;
    }

    public void Remove()
    {
        Delete();
    }

    [ClientRpc]
    public virtual void ComboEffects()
    {
        
    }

    [ClientRpc]
    protected virtual void ShootEffects()
    {
        Game.AssertClient();

        Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle");
        Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle1");

        ViewModelEntity?.SetAnimParameter("fire", true);
    }

    [ClientRpc]
    public virtual void DryFire()
    {

    }

    public IEnumerable<TraceResult> TraceBullets(Vector3 start, Vector3 end, float radius = 2.0f)
    {
        bool underWater = Trace.TestPoint(start, "water");

        var trace = Trace.Ray(start, end)
                .UseHitboxes()
                .WithAnyTags("solid", "player", "npc", "glass")
                .Ignore(Owner)
                .Size(radius);

        if (!underWater)
            trace = trace.WithAnyTags("water");

        var tr = trace.Run();

        if (tr.Hit)
            yield return tr;
    }

    public IEnumerable<TraceResult> TraceMelee(Vector3 start, Vector3 end, float radius = 2.0f)
    {
        var trace = Trace.Ray(start, end)
                .UseHitboxes()
                .WithAnyTags("solid", "player", "npc", "glass")
                .Ignore(this);

        var tr = trace.Run();

        if (tr.Hit)
        {
            yield return tr;
        }
        else
        {
            trace = trace.Size(radius);

            tr = trace.Run();

            if (tr.Hit)
            {
                yield return tr;
            }
        }
    }

    public virtual void ShootBullet(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize)
    {
        var forward = dir;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;

        foreach (var tr in TraceBullets(pos, pos + forward * 5000, bulletSize))
        {
            tr.Surface.DoBulletImpactSurface(tr);

            if (!Game.IsServer) continue;
            if (!tr.Entity.IsValid()) continue;

            using (Prediction.Off())
            {
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 100 * force, damage)
                    .UsingTraceResult(tr)
                    .WithAttacker(Owner)
                    .WithWeapon(this);

                tr.Entity.TakeDamage(damageInfo);
            }
        }
    }

    public virtual void ShootBullets(int numBullets, float spread, float force, float damage, float bulletSize)
    {
        var pos = Owner.AimRay.Position;
        var dir = Owner.Rotation.Forward;

        for (int i = 0; i < numBullets; i++)
        {
            ShootBullet(pos, dir, spread, force / numBullets, damage, bulletSize);
        }
    }

    public static void Explosion(Entity weapon, Entity owner, Vector3 position, float radius, float damage, float forceScale)
    {
        // Effects
        Sound.FromWorld("grenade_explode", position);
        Particles.Create("particles/explosion/barrel_explosion/explosion_barrel.vpcf", position);

        // Damage, etc
        var overlaps = Entity.FindInSphere(position, radius);

        foreach (var overlap in overlaps)
        {
            if (overlap is not ModelEntity ent || !ent.IsValid())
                continue;

            if (ent.LifeState != LifeState.Alive)
                continue;

            if (!ent.PhysicsBody.IsValid())
                continue;

            if (ent.IsWorld)
                continue;

            var targetPos = ent.PhysicsBody.MassCenter;

            var dist = Vector3.DistanceBetween(position, targetPos);
            if (dist > radius)
                continue;

            var tr = Trace.Ray(position, targetPos)
                .Ignore(weapon)
                .WorldOnly()
                .Run();

            if (tr.Fraction < 0.98f)
                continue;

            var distanceMul = 1.0f - Math.Clamp(dist / radius, 0.0f, 1.0f);
            var dmg = damage * distanceMul;
            var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
            var forceDir = (targetPos - position).Normal;

            var damageInfo = DamageInfo.FromExplosion(position, forceDir * force, dmg)
                .WithWeapon(weapon)
                .WithAttacker(owner);

            ent.TakeDamage(damageInfo);
        }
    }
}