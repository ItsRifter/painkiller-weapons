using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class GrabTrigger : ModelEntity
{
    public override void Spawn()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.Spawn();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Tags.Add("trigger");
        RuntimeHelpers.EnsureSufficientExecutionStack();
        SetTriggerSize(16f);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.Transmit = TransmitType.Never;
    }

    [Description("Set the trigger radius. Default is 16.")]
    public void SetTriggerSize(float radius)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, new Capsule(Vector3.Zero, Vector3.One * 0.1f, radius));
    }
}
