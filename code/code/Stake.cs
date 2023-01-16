using Sandbox;
using System;

namespace pkweapons;

[Library("pk_stake")]
[HideInEditor]
partial class Stake : ModelEntity
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_stake.vmdl");

    bool Stuck;
    float gravRot = 0;
    float gravity = 0;
    Rotation rot;
    public StakeGrenade Grenade;

    public override void Spawn()
    {
        base.Spawn();

        Model = WorldModel;
    }

    [Event.Tick.Server]
    public virtual void Tick()
    {
        if (!Game.IsServer)
            return;

        if (Stuck)
            return;

        gravity += 300.0f * Time.Delta;
        gravRot += 0.5f * Time.Delta;

        float Speed = 50.0f;
        var velocity = Velocity * Speed;
        velocity.z -= gravity;
        
        var start = Position;
        var end = start + velocity * Time.Delta;

        rot = Rotation.FromPitch(gravRot);

        var tr = Trace.Ray(start, end)
                .UseHitboxes()
                .Ignore(Owner)
                .Ignore(this)
                .WithoutTags("trigger")
                .Run();

        if(tr.Entity is StakeGrenade grenade)
        {
            grenade.SetParent(this);
            grenade.IsParented = true;
            this.Grenade = grenade;
            PlaySound("stakegun_combo");
            return;
        }

        if (tr.Hit)
        {
            Stuck = true;
            Position = tr.EndPosition + Rotation.Forward * -1;

            if (tr.Entity.IsValid() )
            {
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, tr.Direction * 200, 20)
                .UsingTraceResult(tr)
                .WithAttacker(Owner)
                .WithWeapon(this);

                tr.Entity.TakeDamage(damageInfo);
            }

            SetParent(tr.Entity, tr.Bone);
            Owner = null;

            tr.Normal = Rotation.Forward * -1;
            tr.Surface.DoBulletImpactSurface(tr);
            velocity = default;

            if(this.Grenade != null)
            {
                PKWepBase.Explosion(this, Owner, Position, 400, 100, 1.0f);
                Delete();
            }

            _ = DeleteAsync(30.0f);
        }
        else
        {
            Position = end;
            Rotation *= rot;
        }
    }
}