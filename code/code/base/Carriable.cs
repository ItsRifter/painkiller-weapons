using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class BaseCarriable : AnimatedEntity
{
    public virtual string ViewModelPath => null;

    public BaseVM ViewModelEntity { get; protected set; }

    [Description("Utility - return the entity we should be spawning particles from etc")]
    public virtual ModelEntity EffectEntity
    {
        get
        {
            if (!ViewModelEntity.IsValid() || !IsFirstPersonMode)
            {
                return this;
            }

            return ViewModelEntity;
        }
    }

    public override void Spawn()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.Spawn();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.PhysicsEnabled = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.UsePhysicsCollision = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.EnableHideInFirstPerson = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.EnableShadowInFirstPerson = true;
    }

    public virtual bool CanCarry(Entity carrier)
    {
        return true;
    }

    public virtual void OnCarryStart(Entity carrier)
    {
        if (!Game.IsClient)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            SetParent(carrier, boneMerge: true);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Owner = carrier;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.EnableAllCollisions = false;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.EnableDrawing = false;
        }
    }

    /*public virtual void SimulateAnimator(CitizenAnimationHelper anim)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        anim.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        anim.Handedness = CitizenAnimationHelper.Hand.Both;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        anim.AimBodyWeight = 1f;
    }*/

    public virtual void OnCarryDrop(Entity dropper)
    {
        if (!Game.IsClient)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            SetParent(null);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Owner = null;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.EnableDrawing = true;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.EnableAllCollisions = true;
        }
    }

    [Description("This entity has become the active entity. This most likely means a player was carrying it in their inventory and now has it in their hands.")]
    public virtual void ActiveStart(Entity ent)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.EnableDrawing = true;
        if (IsLocalPawn)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyViewModel();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyHudElements();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            CreateViewModel();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            CreateHudElements();
        }
    }

    [Description("This entity has stopped being the active entity. This most likely means a player was holding it but has switched away or dropped it (in which case dropped = true)")]
    public virtual void ActiveEnd(Entity ent, bool dropped)
    {
        if (!dropped)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.EnableDrawing = false;
        }

        if (Game.IsClient)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyViewModel();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyHudElements();
        }
    }

    protected override void OnDestroy()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.OnDestroy();
        if (Game.IsClient && ViewModelEntity.IsValid())
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyViewModel();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            DestroyHudElements();
        }
    }

    [Description("Create the viewmodel. You can override this in your base classes if you want to create a certain viewmodel entity.")]
    public virtual void CreateViewModel()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Game.AssertClient("CreateViewModel");
        if (!string.IsNullOrEmpty(ViewModelPath))
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ViewModelEntity = new BaseVM();
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ViewModelEntity.Position = Position;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ViewModelEntity.Owner = Owner;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ViewModelEntity.EnableViewmodelRendering = true;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ViewModelEntity.SetModel(ViewModelPath);
        }
    }

    [Description("We're done with the viewmodel - delete it")]
    public virtual void DestroyViewModel()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        ViewModelEntity?.Delete();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        ViewModelEntity = null;
    }

    public virtual void CreateHudElements()
    {
    }

    public virtual void DestroyHudElements()
    {
    }
}
