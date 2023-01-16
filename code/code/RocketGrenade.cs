using Sandbox;
using System.Threading.Tasks;

namespace pkweapons;

[Library("pk_chaingun_rocket")]
partial class RocketGrenade : ModelEntity
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_grenade.vmdl");

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

        float Speed = 10000.0f;
        var velocity = Rotation.Forward * Speed;

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
            PKWepBase.Explosion(this, Owner, Position, 400, 100, 1.0f);
            Delete();
        }

        Position = end;
    }
}