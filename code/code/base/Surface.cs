using Sandbox.Internal;
using Sandbox.ModelEditor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public static class SandboxBaseExtensions
{
    [Description("Sets the procedural hit creation parameters for the animgraph node, which makes the  model twitch according to where it got hit.   The parameters set are  \tbool hit \tint hit_bone \tvector hit_offset \tvector hit_direction \tvector hit_strength")]
    public static void ProceduralHitReaction(this AnimatedEntity self, DamageInfo info, float damageScale = 1f)
    {
        Vector3 vector = self.GetBoneTransform(info.BoneIndex).PointToLocal(info.Position);
        if (vector == Vector3.Zero)
        {
            vector = Vector3.One;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.SetAnimParameter("hit", value: true);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.SetAnimParameter("hit_bone", info.BoneIndex);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.SetAnimParameter("hit_offset", vector);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.SetAnimParameter("hit_direction", info.Force.Normal);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.SetAnimParameter("hit_strength", info.Force.Length / 1000f * damageScale);
    }

    [Description("Copy the bones from the target entity, but at the current entity's position and rotation")]
    public static void CopyBonesFrom(this Entity self, Entity ent)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.CopyBonesFrom(ent, self.Position, self.Rotation);
    }

    [Description("Copy the bones from the target entity, but at this position and rotation instead of the target entity's")]
    public static void CopyBonesFrom(this Entity self, Entity ent, Vector3 pos, Rotation rot, float scale = 1f)
    {
        ModelEntity modelEntity = self as ModelEntity;
        if (modelEntity == null)
        {
            return;
        }

        ModelEntity modelEntity2 = ent as ModelEntity;
        if (modelEntity2 != null)
        {
            if (modelEntity.BoneCount != modelEntity2.BoneCount)
            {
                GlobalGameNamespace.Log.Info($"CopyBonesFrom: Bone count doesn't match - {modelEntity2.BoneCount} vs {modelEntity.BoneCount}");
            }

            Vector3 position = modelEntity2.Position;
            Rotation inverse = modelEntity2.Rotation.Inverse;
            int num = Math.Min(modelEntity.BoneCount, modelEntity2.BoneCount);
            for (int i = 0; i < num; i++)
            {
                Transform boneTransform = modelEntity2.GetBoneTransform(i);
                RuntimeHelpers.EnsureSufficientExecutionStack();
                boneTransform.Position = (boneTransform.Position - position) * inverse * rot + pos;
                RuntimeHelpers.EnsureSufficientExecutionStack();
                boneTransform.Rotation = rot * (inverse * boneTransform.Rotation);
                RuntimeHelpers.EnsureSufficientExecutionStack();
                boneTransform.Scale = scale;
                RuntimeHelpers.EnsureSufficientExecutionStack();
                modelEntity.SetBoneTransform(i, boneTransform);
            }
        }
    }

    [Description("Set the velocity of the ragdoll entity by working out the bone positions of from delta seconds ago")]
    public static void SetRagdollVelocityFrom(this Entity self, Entity fromEnt, float delta = 0.1f, float linearAmount = 1f, float angularAmount = 1f)
    {
        if (delta == 0f)
        {
            return;
        }

        ModelEntity modelEntity = self as ModelEntity;
        if (modelEntity == null)
        {
            return;
        }

        ModelEntity modelEntity2 = fromEnt as ModelEntity;
        if (modelEntity2 == null)
        {
            return;
        }

        Transform[] array = modelEntity2.ComputeBones(0f);
        Transform[] array2 = modelEntity2.ComputeBones(0f - delta);
        for (int i = 0; i < modelEntity2.BoneCount; i++)
        {
            PhysicsBody bonePhysicsBody = modelEntity.GetBonePhysicsBody(i);
            if (bonePhysicsBody != null)
            {
                if (linearAmount > 0f)
                {
                    Vector3 localMassCenter = bonePhysicsBody.LocalMassCenter;
                    Vector3 vector = array2[i].TransformVector(localMassCenter);
                    Vector3 velocity = (array[i].TransformVector(localMassCenter) - vector) * (linearAmount / delta);
                    RuntimeHelpers.EnsureSufficientExecutionStack();
                    bonePhysicsBody.Velocity = velocity;
                }

                if (angularAmount > 0f)
                {
                    Rotation rotation = Rotation.Difference(array2[i].Rotation, array[i].Rotation);
                    RuntimeHelpers.EnsureSufficientExecutionStack();
                    bonePhysicsBody.AngularVelocity = new Vector3(rotation.x, rotation.y, rotation.z) * (angularAmount / delta);
                }
            }
        }
    }

    [Description("Returns all game data nodes that derive from given class/interface, and are present on the model. Does NOT support data nodes that allow multiple entries.")]
    public static List<T> GetAllData<T>(this Model self)
    {
        List<T> val = new List<T>();
        foreach (TypeDescription type in GlobalGameNamespace.TypeLibrary.GetTypes<T>())
        {
            GameDataAttribute attribute = type.GetAttribute<GameDataAttribute>();
            if (attribute != null && attribute.AllowMultiple && self.TryGetData(type.TargetType, out var data))
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                val.Add((T)data);
            }
        }

        return val;
    }

    [Description("Create a particle effect and play an impact sound for this surface being hit by a bullet")]
    public static Particles DoBulletImpactSurface(this Surface self, TraceResult tr)
    {
        if (!Prediction.FirstTime)
        {
            return null;
        }

        string text = Game.Random.FromArray(self.ImpactEffects.BulletDecal);
        Surface baseSurface = self.GetBaseSurface();
        while (string.IsNullOrWhiteSpace(text) && baseSurface != null)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            text = Game.Random.FromArray(baseSurface.ImpactEffects.BulletDecal);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            baseSurface = baseSurface.GetBaseSurface();
        }

        if (!string.IsNullOrWhiteSpace(text) && GlobalGameNamespace.ResourceLibrary.TryGet<DecalDefinition>(text, out var resource))
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Decal.Place(resource, tr);
        }

        string bullet = self.Sounds.Bullet;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        baseSurface = self.GetBaseSurface();
        while (string.IsNullOrWhiteSpace(bullet) && baseSurface != null)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            bullet = baseSurface.Sounds.Bullet;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            baseSurface = baseSurface.GetBaseSurface();
        }

        if (!string.IsNullOrWhiteSpace(bullet))
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Sound.FromWorld(bullet, tr.EndPosition);
        }

        string text2 = Game.Random.FromArray(self.ImpactEffects.Bullet);
        if (string.IsNullOrWhiteSpace(text2))
        {
            text2 = Game.Random.FromArray(self.ImpactEffects.Regular);
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        baseSurface = self.GetBaseSurface();
        while (string.IsNullOrWhiteSpace(text2) && baseSurface != null)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            text2 = Game.Random.FromArray(baseSurface.ImpactEffects.Bullet);
            if (string.IsNullOrWhiteSpace(text2))
            {
                text2 = Game.Random.FromArray(baseSurface.ImpactEffects.Regular);
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();
            baseSurface = baseSurface.GetBaseSurface();
        }

        if (!string.IsNullOrWhiteSpace(text2))
        {
            Particles particles = Particles.Create(text2, tr.EndPosition);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            particles.SetForward(0, tr.Normal);
            return particles;
        }

        return null;
    }

    [Description("Create a footstep effect")]
    public static void DoFootstep(this Surface self, Entity ent, TraceResult tr, int foot, float volume)
    {
        string text = ((foot == 0) ? self.Sounds.FootLeft : self.Sounds.FootRight);
        if (!string.IsNullOrWhiteSpace(text))
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Sound.FromWorld(text, tr.EndPosition).SetVolume(volume);
        }
        else if (self.GetBaseSurface() != null)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.GetBaseSurface().DoFootstep(ent, tr, foot, volume);
        }
    }

    [Description("Add a vertex using this position and everything else from Default")]
    public static void Add(this VertexBuffer self, Vector3 pos)
    {
        Vertex @default = self.Default;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        @default.Position = pos;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Add(@default);
    }

    [Description("Add a vertex using this position and UV, and everything else from Default")]
    public static void Add(this VertexBuffer self, Vector3 pos, Vector2 uv)
    {
        Vertex @default = self.Default;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        @default.Position = pos;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        @default.TexCoord0.x = uv.x;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        @default.TexCoord0.y = uv.y;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Add(@default);
    }

    [Description("Add a triangle to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddTriangle(this VertexBuffer self, Vertex a, Vertex b, Vertex c)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Add(a);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Add(b);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Add(c);
        if (self.Indexed)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.AddTriangleIndex(3, 2, 1);
        }
    }

    [Description("Add a quad to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddQuad(this VertexBuffer self, Rect rect)
    {
        Vector2 position = rect.Position;
        Vector2 size = rect.Size;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(position, new Vector2(position.x + size.x, position.y), position + size, new Vector2(position.x, position.y + size.y));
    }

    [Description("Add a quad to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddQuad(this VertexBuffer self, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        if (self.Indexed)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(b);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(d);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.AddTriangleIndex(4, 3, 2);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.AddTriangleIndex(2, 1, 4);
        }
        else
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(b);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(d);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a);
        }
    }

    [Description("Add a quad to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddQuad(this VertexBuffer self, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        if (self.Indexed)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a, new Vector2(0f, 0f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(b, new Vector2(1f, 0f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c, new Vector2(1f, 1f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(d, new Vector2(0f, 1f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.AddTriangleIndex(4, 3, 2);
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.AddTriangleIndex(2, 1, 4);
        }
        else
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a, new Vector2(0f, 0f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(b, new Vector2(1f, 0f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c, new Vector2(1f, 1f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(c, new Vector2(1f, 1f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(d, new Vector2(0f, 1f));
            RuntimeHelpers.EnsureSufficientExecutionStack();
            self.Add(a, new Vector2(0f, 0f));
        }
    }

    [Description("Add a quad to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddQuad(this VertexBuffer self, Ray origin, Vector3 width, Vector3 height)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Default.Normal = origin.Forward;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        ref Vertex @default = ref self.Default;
        Vector3 v = width.Normal;
        @default.Tangent = new Vector4(in v, 1f);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(origin.Position - width - height, origin.Position + width - height, origin.Position + width + height, origin.Position - width + height);
    }

    [Description("Add a cube to the vertex buffer. Will include indices if they're enabled.")]
    public static void AddCube(this VertexBuffer self, Vector3 center, Vector3 size, Rotation rot, Color32 color = default(Color32))
    {
        Color32 color2 = self.Default.Color;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Default.Color = color;
        Vector3 vector = rot.Forward * size.x * 0.5f;
        Vector3 vector2 = rot.Left * size.y * 0.5f;
        Vector3 vector3 = rot.Up * size.z * 0.5f;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center + vector, vector.Normal), vector2, vector3);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center - vector, -vector.Normal), vector2, -vector3);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center + vector2, vector2.Normal), -vector, vector3);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center - vector2, -vector2.Normal), vector, vector3);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center + vector3, vector3.Normal), vector, vector2);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.AddQuad(new Ray(center - vector3, -vector3.Normal), vector, -vector2);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        self.Default.Color = color2;
    }
}