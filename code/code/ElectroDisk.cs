using Sandbox;
using System;

namespace pkweapons;

[Library("pk_electrodisk")]
[HideInEditor]
partial class ElectroDisk : ModelEntity
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_electrodisk.vmdl");

    bool Stuck;

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
        {
            DamageInfo dmgInfo = new DamageInfo();
            dmgInfo.Damage = 1.0f;
            dmgInfo.WithAttacker(Owner, this);

            foreach (var ent in FindInSphere(Position, 48.0f))
            {
                if (ent == Owner)
                    continue;

                ent.TakeDamage(dmgInfo);
            }

            return;
        }

        float Speed = 150.0f;
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
            Stuck = true;
            Position = tr.EndPosition + Rotation.Forward * -1;

            SetParent(tr.Entity, tr.Bone);
            Owner = null;

            tr.Normal = Rotation.Forward * -1;
            tr.Surface.DoBulletImpactSurface(tr);
            velocity = default;

            _ = DeleteAsync(5.0f);
        }
        else
        {
            Position = end;
        }
    }
}