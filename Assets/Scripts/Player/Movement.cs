using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region Variables
    public MovementStats MoveStats;
    [SerializeField] private Collider2D _groundColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    //Animator Variables
    public Animator _animator;

    //Movement Variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;
    private Vector2 mouseVector;

    //Collision Variables
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    [SerializeField] private bool _isGrounded;
    private bool _headBumped;

    //Jump Variables
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;
    private bool _jumpFromGrapple;

    //Apex Variables
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //Jump Buffer Variables
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    //Coyote Time Variables
    private float _coyoteTimer;

    //Gravity
    private float _gravity;

    //Grapple Variables
    [SerializeField] private Transform _grapplerTip;
    [SerializeField] private LineRenderer lr;
    private bool _isGrappling;
    private Vector2 _grapplePoint;
    private bool _startGrapple;
    private bool _grappleThrowing;
    private bool _pointHit;
    private float _throwingTimer; //Timer to track how long since grapple has been thrown (used to animate grapple being thrown)
    private bool _beingDrawn;

    //Grapple Buffer Variables
    private float _grappleBufferTimer; //Buffer for when grapple can be thrown again
    #endregion

    private void Awake()
    {
        _isFacingRight = true;
        _gravity = MoveStats.Gravity;

        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        GrappleCheck();
        JumpChecks();
    }

    private void FixedUpdate() 
    {
        mouseVector = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y);

        TurnCheck(mouseVector);
        CollisionChecks();
        StartGrapple();

        //If grapple is being thrown, draw grapple line renderer
        if ( _grappleThrowing)
        {
            _throwingTimer += Time.fixedDeltaTime;

            //Not throwing anymore, then stop drawing the grapple further from player
            if(_throwingTimer > MoveStats.GrappleDelayTime)
            {
                _grappleThrowing = false;
            }
    
            //If grapple being thrown, extend grapple from player
            if(_grappleThrowing)
            {
                float grappleThrowProgress = _throwingTimer / MoveStats.GrappleDelayTime;
                Vector3 grapplingDirection = new Vector3(_grapplePoint.x - _grapplerTip.position.x, _grapplePoint.y - _grapplerTip.position.y, 0).normalized;
                lr.SetPosition(1, new Vector3(_grapplerTip.position.x + grapplingDirection.x * MoveStats.GrappleRange * grappleThrowProgress, _grapplerTip.position.y + grapplingDirection.y * MoveStats.GrappleRange * grappleThrowProgress, 0));
                
                if(_pointHit)
                {
                    if(Vector3.Distance(lr.GetPosition(1), lr.GetPosition(0)) >= Vector3.Distance(_grapplePoint, lr.GetPosition(0)))
                    {
                        lr.SetPosition(1, _grapplePoint);
                        _grappleThrowing = false;
                        _beingDrawn = true;
                    }
                }
            }
        }

        //If grapple is active and point is hit, wait til throwing timer ends, then execute grapple
        if (_pointHit && !_grappleThrowing)
        {
            if(Vector3.Distance(_rb.transform.position, _grapplePoint) <= 0.8f)
            {
                _beingDrawn = false;
            }

            //If player jumps from grapple, stop grapple and execute jump
            if(_jumpFromGrapple)
            {
                StopGrapple();

                _moveVelocity = _rb.velocity;
                _jumpFromGrapple = false;
                _isJumping = false;
                _isFalling = false;
                _isFastFalling = false;
                _fastFallTime = 0f;
                _isPastApexThreshold = false;
                _beingDrawn = false;
            }

            //Pull player to grapple point
            else {
                Vector2 targetVelocity = new Vector2(_grapplePoint.x - transform.position.x, _grapplePoint.y - transform.position.y).normalized * MoveStats.GrappleSpeed;
                _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, MoveStats.GrappleForce * Time.fixedDeltaTime);
                _rb.velocity = new Vector2(_moveVelocity.x, _moveVelocity.y);
                return; //Stops player movement while grappled
            }
        }

        Jump();
        
        if(_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else { Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement); }
    }

    private void LateUpdate()
    {
        //Update grapple line renderer start position
        if (_isGrappling)
        {
            lr.SetPosition(0, _grapplerTip.position);
        }
    }

    private void OnDrawGizmos()
    {
        if (MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.blue);
        }
    }

    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if(_isGrappling && !_grappleThrowing)
        {
            return;
        }

        if(moveInput != Vector2.zero)
        {
            //TurnCheck(moveInput);
            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
               _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            _isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void Turn(bool turnRight)
    {
        if(turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
    #endregion

    #region Jump

    private void JumpChecks()
    {
        //When jump button pressed
        if (InputManager.JumpPressed)
        {
            if(_isGrappling && !_grappleThrowing)
            {
                _jumpFromGrapple = true;
                _numberOfJumpsUsed += 1;

                //Only Initiate jump for grapple if player is at the grapple position
                if(!_beingDrawn)
                {
                    _numberOfJumpsUsed = 0;
                    Debug.Log("E");
                    InitiateJump(1);
                }
            }
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        //When jump button released
        if(InputManager.JumpReleased)
        {
            if (_jumpBufferTimer > 0)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate jump with jump buffering and coyote time
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Double Jump
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        //Air jump and coyote time after lapsed
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }
        
        //Landing
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;
            
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            Debug.Log("Y");
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        if (!_beingDrawn && _isGrappling && !_grappleThrowing)
        {
            VerticalVelocity = .5f * MoveStats.InitialJumpVelocity;
        } else { VerticalVelocity = MoveStats.InitialJumpVelocity; }
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor) 
    {
        Vector2 startPosition = new Vector2(_groundColl.bounds.center.x, _groundColl.bounds.min.y);
        Vector2 previousPosition = startPosition;

        float speed = 0f;

        if (MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }

        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);
        Gizmos.color = gizmoColor;
        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution;
        for (int i = 0; i<MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPosition;
            
            if (simulationTime < MoveStats.TimeTillJumpApex) //Ascending
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime) //Apex hang time
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime; //No vertical movement during hang time
            }
            else //Descending
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime; //Horizontal movement during hang time
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, MoveStats.Gravity) * descendTime * descendTime;
            }

            drawPosition = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPosition - previousPosition, Vector2.Distance(previousPosition, drawPosition), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    //If a hit is detected, stop drawing the arc at the hit point
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }
        
            Gizmos.DrawLine(previousPosition, drawPosition);
            previousPosition = drawPosition;
        }
    }
    
    private void Jump()
    {
        //Apply gravity while jumping
        if (_isJumping)
        {
            Debug.Log("Jumping");
            if (_headBumped)
            {
                //Check if player bonks head
                _isFastFalling = true;
            }

            //Gravity on ascent
            if (VerticalVelocity >= 0f)
            { 
                //Apex controls
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                
                //gravity while ascending, but before apex
                else
                {
                    VerticalVelocity +=MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            } 
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //Jump cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }
        
        //Normal gravity Wwile falling
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        //Clamp vertical velocity
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 69f);
        _rb.velocity =  new Vector2(_rb.velocity.x, VerticalVelocity);
    }
    #endregion

    #region Grapple

    private void GrappleCheck()
    {
        if (InputManager.GrapplePressed)
        {
            _startGrapple = true;
        }
    }

    //Start grapple process, before movement
    private void StartGrapple()
    {
        if (_startGrapple)
        {   
            _startGrapple = false;
            if (_grappleBufferTimer > 0 || _isGrappling) return; 

            _isGrappling = true;
            _grappleThrowing = true;
            _throwingTimer = 0f;

            Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            Vector2 grappleDirection = new Vector3(mousePosition.x - _grapplerTip.position.x, mousePosition.y - _grapplerTip.position.y).normalized;
            RaycastHit2D hit = Physics2D.Raycast(_grapplerTip.position, grappleDirection, MoveStats.GrappleRange, MoveStats.GroundLayer);
            if(hit.collider != null)
            {
                _grapplePoint = hit.point;
                _pointHit = true;
            }
            else
            {
                _grapplePoint = new Vector2(_grapplerTip.position.x + (grappleDirection * MoveStats.GrappleRange).x, _grapplerTip.position.y + (grappleDirection * MoveStats.GrappleRange).y);
                Invoke(nameof(StopGrapple), MoveStats.GrappleDelayTime);
            }
    
            lr.enabled = true;
            lr.SetPosition(0, _grapplerTip.position);
        }
    }
    
    private void StopGrapple()
    {
        _isGrappling = false;
        _pointHit = false;
        _grappleBufferTimer = MoveStats.GrapplingCd;

        lr.enabled = false;
    }
    
    #endregion

    #region Collision

    private void IsGrounded()
    {
        Vector2 boxCastCenter = new Vector2(_groundColl.bounds.center.x, _groundColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_groundColl.bounds.size.x, MoveStats.GroundDetectionDistance);

        _groundHit = Physics2D.BoxCast(boxCastCenter, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionDistance, MoveStats.GroundLayer);
        if(_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        #region Debug Visualization
        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastCenter.x - boxCastSize.x / 2, boxCastCenter.y), Vector2.down * MoveStats.GroundDetectionDistance, rayColor);
            Debug.DrawRay(new Vector2(boxCastCenter.x + boxCastSize.x / 2, boxCastCenter.y), Vector2.down * MoveStats.GroundDetectionDistance, rayColor);
            Debug.DrawRay(new Vector2(boxCastCenter.x - boxCastSize.x / 2, boxCastCenter.y - MoveStats.GroundDetectionDistance), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void BonkedHead()
    {
        Vector2 boxCastCenter = new Vector2(_groundColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_groundColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionDistance);
        
        #region Debug Visualization
        
        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (_headBumped)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastCenter.x - boxCastSize.x / 2 * headWidth, boxCastCenter.y), Vector2.up * MoveStats.HeadDetectionDistance, rayColor);
            Debug.DrawRay(new Vector2(boxCastCenter.x + (boxCastSize.x / 2) * headWidth, boxCastCenter.y), Vector2.up * MoveStats.HeadDetectionDistance, rayColor);
            Debug.DrawRay(new Vector2(boxCastCenter.x - boxCastSize.x / 2 * headWidth, boxCastCenter.y + MoveStats.HeadDetectionDistance), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BonkedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        _grappleBufferTimer -= Time.deltaTime;

        if(!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else { _coyoteTimer = MoveStats.JumpCoyoteTime; }
    }
    #endregion
}
