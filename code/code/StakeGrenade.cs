using Sandbox;
using System.Threading.Tasks;

namespace pkweapons;

[Library("pk_stakegun_grenade")]
partial class StakeGrenade : ModelEntity
{
    public static readonly Model WorldModel = Model.Load("models/weapons/painkiller/w_grenade.vmdl");

    public bool IsParented;

    public override void Spawn()
    {
        base.Spawn();

        Model = WorldModel;
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
    }

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        base.OnPhysicsCollision(eventData);

        PlaySound("grenade_bounce");
    }

    [Event.Tick.Server]
    public virtual void Tick()
    {
        if (!Game.IsServer)
            return;

        var ents = FindInSphere(Position, 32.0f);

        foreach (var ent in ents)
        { 
            PKWepBase.Explosion(this, Owner, Position, 400, 75, 1.0f);
            Delete(); 
        }
    }

    public async Task BlowIn(float seconds)
    {
        await Task.DelaySeconds(seconds);

        if (!IsValid)
            return;

        if (IsParented)
            return;

        PKWepBase.Explosion(this, Owner, Position, 400, 100, 1.0f);
        Delete();
    }
}