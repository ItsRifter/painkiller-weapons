using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox;
public class BaseVM : AnimatedEntity
{
    public static List<BaseVM> AllViewModels = new List<BaseVM>();

    public BaseVM()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        AllViewModels.Add(this);
    }

    public override void Spawn()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.Spawn();
    }

    protected override void OnDestroy()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.OnDestroy();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        AllViewModels.Remove(this);
    }

    public override void OnNewModel(Model model)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.OnNewModel(model);
    }

    [Event.Client.PostCamera]
    public virtual void PlaceViewmodel()
    {
        if (!Game.IsRunningInVR)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Position = Camera.Position;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Rotation = Camera.Rotation;
        }
    }

    public override Sound PlaySound(string soundName, string attachment)
    {
        if (Owner.IsValid())
        {
            return Owner.PlaySound(soundName, attachment);
        }

        return base.PlaySound(soundName, attachment);
    }
}