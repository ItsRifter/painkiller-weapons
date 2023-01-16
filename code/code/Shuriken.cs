using Sandbox;
using System;

namespace pkweapons;

[Library("pk_shuriken")]
[HideInEditor]
partial class Shuriken : ModelEntity
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_shuriken.vmdl");

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

        float Speed = 100.0f;
        var velocity = Velocity * Speed;
        
        var start = Position;
        var end = start + velocity * Time.Delta;

        var tr = Trace.Ray(start, end)
                .UseHitboxes()
                .Ignore(Owner)
                .Ignore(this)
                .WithoutTags("trigger")
                .Run();

        if (tr.Hit)
        {
            if (tr.Entity.IsValid())
            {
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, tr.Direction * 200, 20)
                .UsingTraceResult(tr)
                .WithAttacker(Owner)
                .WithWeapon(this);

                tr.Entity.TakeDamage(damageInfo);
            }

            Delete();
        }
        else
        {
            Position = end;
        }
    }
}