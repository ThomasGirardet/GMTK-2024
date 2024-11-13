using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

[CreateAssetMenu(menuName = "Player Movement")]
public class MovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 50f)] public float MaxWalkSpeed = 10f; //Max walk speed
    [Range(.25f, 50f)] public float GroundAcceleration = 5f; //Acceleration on the ground
    [Range(.25f, 50f)] public float GroundDeceleration = 20f; //Deceleration on the ground
    [Range(.25f, 50f)] public float AirAcceleration = 5f; //Acceleration in the air
    [Range(.25f, 50f)] public float AirDeceleration = 5f; //Deceleration in the air

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayer; //Layermask for the ground
    public float GroundDetectionDistance = .02f; //Distance to check for ground
    public float HeadDetectionDistance = .02f; //Distance to check for ceiling
    [Range(0f, 1f)] public float HeadWidth = 0.75f; //Width of the head check

    [Header("Jump")]
    public float JumpHeight = 6.5f; //Height of the jump
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f; //Compensation factor for jump height
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 1;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.27f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Grapple")]
    [Range(1f, 50f)] public float GrappleRange = 20f;
    [Range(0f, 1f)] public float GrappleDelayTime = 1f;
    [Range(2f, 30f)] public float GrappleForce = 15f;
    [Range(1f, 50f)] public float GrappleSpeed = 15f;

    [Header("Grapple Cooldown")]
    [Range(0f, 1f)] public float GrapplingCd;

    [Header("Debug")]
    public bool DebugShowIsGroundedBox;
    public bool DebugShowHeadBumpBox;

    [Header("JumpVisualization Tool")]
    public bool ShowWalkJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}
