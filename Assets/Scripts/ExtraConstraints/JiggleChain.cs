using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

/// Jiggle Chain constraint as presented at GDC2019
/// "Introducing the New Animation Rigging Features" - Unity Developer Day
/// NOTE: Implementation is only compatible with Animation Rigging package preview-0.2.1 [Unity 2019.2] and up.

/// JiggleChainJob implements the jiggle chain algorithm that will be executed on the animator thread
[BurstCompile]
public struct JiggleChainJob : IWeightedAnimationJob
{
    const float k_Epsilon = 1e-6f;
    const float k_StiffnessMultiplier = 1f;
    const float k_ForceMultiplier = 62f;

    public struct SimulationProperties
    {
        public float mass;         // Particle mass
        public float decay;        // Controls overshoot by removing some motion momentum
        public float damping;      // Velocity damping factor
        public float stiffness;    // Distance constraint stiffness
        public float restDistance; // Dynamic offset distance used as rest distance for constraint
        public float dt;           // Time step
    }

    public struct DynamicTarget
    {
        public float3 position;
        public float3 velocity;

        public void Reset(float3 position)
        {
            this.position = position;
            velocity = float3.zero;
        }

        public void ApplyDistanceConstraint(ref float3 dynamicPos, float3 pinnedPos, float stiffness, float restDistance)
        {
            // Geometric distance constraint considering a pinned position. All correction to respect constraint is applied to
            // dynamicPos. Since we are not considering the mass of both particles, gravity will not affect the
            // constraint behavior.

            float3 diff = pinnedPos - dynamicPos;
            float sqLength = math.lengthsq(diff);
            if (sqLength > k_Epsilon)
            {
                float length = math.sqrt(sqLength);
                dynamicPos += diff * (stiffness * ((length - restDistance) / length));
            }
        }

        public void Update(float3 targetRoot, float3 target, float3 forces, in SimulationProperties properties)
        {
            // Solver based from the work of Mathias Muller and al. "Position Based Dynamics",
            // 3rd Workshop in Virtual Reality Interactions and Physical Simulation "VRIPHYS" (2006),
            // AGEIA ( Section 3.3 )

            float3 acceleration = (forces - properties.damping * velocity) / properties.mass;
            velocity += acceleration * properties.dt;
            velocity *= properties.decay;

            float3 prevPosition = position;

            position += velocity * properties.dt;
            ApplyDistanceConstraint(ref position, targetRoot, properties.stiffness, properties.restDistance);
            ApplyDistanceConstraint(ref position, target, properties.stiffness, 0f);

            velocity = (position - prevPosition) / properties.dt;
        }
    }

    const float k_FixedDt = 1f / 60f; // 60Hz fixed timestep

    public NativeArray<ReadWriteTransformHandle> chain;
    public float3 localAimDir;
    public float3 localUpDir;

    public NativeArray<DynamicTarget> dynamicTargetAim;
    public NativeArray<DynamicTarget> dynamicTargetRoll;

    // User input properties
    public FloatProperty   massProperty;
    public FloatProperty   stiffnessProperty;
    public FloatProperty   dynamicOffsetProperty;
    public BoolProperty    rollEnabledProperty;
    public Vector3Property gravityProperty;
    public Vector3Property externalForceProperty;
    public FloatProperty   motionDecayProperty;
    public FloatProperty   dampingProperty;

    public SimulationProperties simulationProperties;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        float dynamicOffset = dynamicOffsetProperty.Get(stream);
        bool doRoll = rollEnabledProperty.Get(stream);

        simulationProperties.mass = massProperty.Get(stream);
        simulationProperties.decay = AnimationRuntimeUtils.Square(1f - motionDecayProperty.Get(stream));
        simulationProperties.damping = 1f - dampingProperty.Get(stream);
        simulationProperties.stiffness = AnimationRuntimeUtils.Square(stiffnessProperty.Get(stream) * k_StiffnessMultiplier);
        simulationProperties.restDistance = dynamicOffset;
        simulationProperties.dt = k_FixedDt;

        float3 gravity       = gravityProperty.Get(stream);
        float3 externalForce = externalForceProperty.Get(stream);
        float3 forces        = ((gravity * simulationProperties.mass) + externalForce) * k_ForceMultiplier;

        for (int i = 0, count = chain.Length; i < count; ++i)
        {
            ReadWriteTransformHandle chainHandle = chain[i];
            chainHandle.GetGlobalTR(stream, out Vector3 chainWPos, out Quaternion chainWRot);

            float4x4 tx = float4x4.TRS(chainWPos, chainWRot, new float3(1f));
            float3 aimTarget  = math.transform(tx, (localAimDir * dynamicOffset));
            float3 rollTarget = math.transform(tx, (localUpDir * dynamicOffset));

            var dynamicAim  = dynamicTargetAim[i];
            var dynamicRoll = dynamicTargetRoll[i];

            float streamDt = math.abs(stream.deltaTime);
            if (w > 0f && streamDt > 0f)
            {
                float3 txTranslation = chainWPos;
                quaternion txRotation = chainWRot;

                while (streamDt > 0f)
                {
                    dynamicAim.Update(txTranslation, aimTarget, forces, simulationProperties);
                    if (doRoll)
                    {
                        // Roll particles are not affected by external forces to prevent weird configurations
                        dynamicRoll.Update(txTranslation, rollTarget, float3.zero, simulationProperties);
                    }

                    streamDt -= k_FixedDt;
                }

                // Compute aimDeltaRot in all axis except roll
                float3 rotMask = math.abs(localAimDir);
                quaternion aimDeltaRot = Project(
                    txRotation,
                    FromTo(aimTarget - txTranslation, dynamicAim.position - txTranslation),
                    new float3(1f) - rotMask
                    );

                // Compute rollDeltaRot in roll axis only
                quaternion rollDeltaRot = doRoll ? Project(
                    txRotation,
                    FromTo(rollTarget - txTranslation, dynamicRoll.position - txTranslation),
                    rotMask
                    ) : quaternion.identity;

                chain[i].SetRotation(
                    stream,
                    math.slerp(chain[i].GetRotation(stream),  math.mul(rollDeltaRot, math.mul(aimDeltaRot, txRotation)), w)
                    );
            }
            else
            {
                dynamicAim.Reset(aimTarget);
                dynamicRoll.Reset(rollTarget);
                AnimationRuntimeUtils.PassThrough(stream, chainHandle);
            }

            dynamicTargetAim[i] = dynamicAim;
            dynamicTargetRoll[i] = dynamicRoll;
            chain[i] = chainHandle;
        }
    }

    static quaternion Project(quaternion rot, quaternion deltaRot, float3 mask)
    {
        quaternion invRot = math.inverse(rot);
        quaternion tmp = math.mul(invRot, math.mul(deltaRot, rot));
        tmp.value.x *= mask.x;
        tmp.value.y *= mask.y;
        tmp.value.z *= mask.z;
        return math.mul(rot, math.mul(tmp, invRot));
    }

    static quaternion FromTo(float3 from, float3 to)
    {
        float theta = math.dot(math.normalize(from), math.normalize(to));
        if (theta >= 1f)
            return quaternion.identity;

        if (theta <= -1f)
        {
            float3 axis = math.cross(from, new float3(1f, 0f, 0f));
            if (math.lengthsq(axis) == 0f)
                axis = math.cross(from, math.up());

            return quaternion.AxisAngle(axis, (float)math.PI);
        }

        return quaternion.AxisAngle(math.normalize(math.cross(from, to)), math.acos(theta));
    }
}

/// JiggleChainData contains all necessary data needed by the JiggleChainJob
[Serializable]
public struct JiggleChainData : IAnimationJobData
{
    [SyncSceneToStream] public Transform root;
    public Transform tip;

    [Header("Properties")]
    [SyncSceneToStream] public float mass;
    [SyncSceneToStream, Range(0f, 1f)] public float stiffness;
    [SyncSceneToStream, Range(0.1f, 100f)] public float dynamicOffset;

    public Vector3 localAimVector;
    public Vector3 localUpVector;
    [SyncSceneToStream] public bool rollEnabled;
    
    [Header("Forces")]
    [SyncSceneToStream] public Vector3 gravity;
    [SyncSceneToStream] public Vector3 externalForce;

    [Header("Simulation Settings")]
    [SyncSceneToStream, Range(0f, 1f)] public float damping;
    [SyncSceneToStream, Range(0f, 1f)] public float motionDecay;

    public bool IsValid()
    {
        if (root == null || tip == null || root == tip)
            return false;

        Transform tmp = tip;
        while (tmp != null && tmp != root)
            tmp = tmp.parent;

        return (tmp == root);
    }

    public void SetDefaultValues()
    {
        root = null;
        tip = null;
        mass = 1f;
        stiffness = 0.5f;
        dynamicOffset = 4f;
        localAimVector = new Vector3(1f, 0f, 0f);
        localUpVector = new Vector3(0f, 1f, 0f);
        rollEnabled = false;
        gravity = Vector3.zero;
        externalForce = Vector3.zero;
        motionDecay = 0.1f;
        damping = 0.0f;
    }
}

/// JiggleChainBinder creates and destroys a JiggleChainJob given specified JiggleChainData
public class JiggleChainBinder : AnimationJobBinder<JiggleChainJob, JiggleChainData>
{
    public override JiggleChainJob Create(Animator animator, ref JiggleChainData data, Component component)
    {
        List<Transform> chain = new List<Transform>();
        Transform tmp = data.tip;
        while (tmp != data.root)
        {
            chain.Add(tmp);
            tmp = tmp.parent;
        }
        chain.Reverse();

        var job = new JiggleChainJob();

        job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.dynamicTargetAim = new NativeArray<JiggleChainJob.DynamicTarget>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        job.dynamicTargetRoll = new NativeArray<JiggleChainJob.DynamicTarget>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        job.localAimDir = math.normalize(data.localAimVector);
        job.localUpDir = math.normalize(data.localUpVector);

        float3 localAimOffset = job.localAimDir * data.dynamicOffset;
        float3 localUpOffset  = job.localUpDir  * data.dynamicOffset;
        for (int i = 0; i < chain.Count; ++i)
        {
            job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
            float4x4 tx = float4x4.TRS(chain[i].position, chain[i].rotation, new float3(1f));

            job.dynamicTargetAim[i] = new JiggleChainJob.DynamicTarget() { position = math.transform(tx, localAimOffset), velocity = Vector3.zero };
            job.dynamicTargetRoll[i] = new JiggleChainJob.DynamicTarget() { position = math.transform(tx, localUpOffset), velocity = Vector3.zero };
        }
        
        // Bind dynamic properties
        job.massProperty          = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.mass)));
        job.stiffnessProperty     = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.stiffness)));
        job.dynamicOffsetProperty = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.dynamicOffset)));
        job.rollEnabledProperty   = BoolProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.rollEnabled)));
        job.gravityProperty       = Vector3Property.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.gravity)));
        job.externalForceProperty = Vector3Property.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.externalForce)));
        job.motionDecayProperty   = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.motionDecay)));
        job.dampingProperty       = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data.damping)));

        return job;
    }

    public override void Destroy(JiggleChainJob job)
    {
        job.chain.Dispose();
        job.dynamicTargetAim.Dispose();
        job.dynamicTargetRoll.Dispose();
    }
}

/// JiggleChain constraint component can be defined given it's job, data and binder
[DisallowMultipleComponent]
public class  JiggleChain : RigConstraint<JiggleChainJob, JiggleChainData, JiggleChainBinder>
{ }