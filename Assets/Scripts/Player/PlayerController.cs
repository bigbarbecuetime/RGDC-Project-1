using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace RGDCP1.Player
{
    // TODO: Seperate input component from player class, and create a input manager for multiple local players supported

    /// <summary>
    /// PlayerController is used to determine the physical movements, and movement state of the player in the world.
    /// It is a physics based controller relying on a rigidbody2d.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        // TODO: State - Player Controller will eventually use a Finite State Machine to track its current state. (Ex. Falling, Running)

        /// <summary>
        /// The minimum value that can be entered in unity's inspector for the script.
        /// </summary>
        private const int MIN_ATTRIBUTE_VALUE = 0;

        // TODO: Movement - Update to support more than just -1, 0, 1 should work with all floating point values
        /// <summary>
        /// Value used to determine if trying to move left or right in the x axis.
        /// Fixed between -1 - 1.
        /// </summary>
        private float xMovementAxis = 0;

        /// <summary>
        ///  Value used to determine if trying to jump.
        /// </summary>
        private bool jumpPressed;

        
        /// <summary>
        /// Used when jumping, to time how long has been jumping. And when a new jump can begin.
        /// </summary>
        private float jumpTimer = 0;

        // TODO: State - Handle with state machine
        /// <summary>
        /// Stores if the player is currently jumping.
        /// </summary>
        private bool isJumping = false;

        /// <summary>
        /// Timer for how long has the player been in coyote time.
        /// </summary>
        private float coyoteTimer = 0;

        // TODO: State - state should be stored in a seperate state machine class
        /// <summary>
        /// Stores if the player is currently grounded or not.
        /// The player is considered grounded when, the player's "feet" are touching a surface below it.
        /// </summary>
        private bool isGrounded = false;

        /// <summary>
        /// Threashold to see when we need to slow down the player automatically, when landed and no input is pressed, calculated based on deceleration.
        /// </summary>
        private float slowdownThreshold;

        /// <summary>
        /// Direction of the "head" of the player, its relative up.
        /// </summary>
        private Vector2 playerUp = Vector3.up;

        /// <summary>
        /// Direction of the attack cone
        /// Z rotation
        /// </summary>
        private float aimAngle = 0f;

        /// <summary>
        /// Rigidbody used for the player's movement.
        /// </summary>
        private Rigidbody2D playerRigidbody;

        [SerializeField]
        [SerializeReference]
        private GameObject attackCone;

        // Camera settings section.
        [Header("Camera Settings")]

        // Movement settings section.
        [Header("Movement Settings")]

        /// <summary>
        /// Maximum x velocity player can have.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxXVelocity;

        /// <summary>
        /// Should the player auto slowdown when in the air?
        /// </summary>
        [SerializeField]
        private bool autoSlowdownInAir;
        
        // Jump settings section.
        [Header("Jump Settings")]

        /// <summary>
        /// Curve read for the acceleration to be used.
        /// </summary>
        [SerializeField]
        private AnimationCurve jumpAccelerationCurve;

        /// <summary>
        /// The acceleration the player will experience when jumping.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float jumpAccelerationFactor;

        /// <summary>
        /// Maximum jump time allowed, in seconds.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxJumpTime;

        /// <summary>
        /// Maximum time of falling off of a platform, that jumping is still allowed.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float coyoteTime;

        /// <summary>
        /// Maximum angle allowed to be considered if the player is "touching the ground" upon collision.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxGroundCollisionAngle;

        // Air movement settings section.
        [Header("Air Movement")]

        /// <summary>
        /// Curve read for the acceleration to be used.
        /// </summary>
        [SerializeField]
        private AnimationCurve airAccelerationCurve;

        /// <summary>
        /// Acceleration factor in m/s^2 for when the player is in the air.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float airAccelerationFactor;

        /// <summary>
        /// Curve read for the deceleration to be used.
        /// Decelerations are evaulated in reverse, 1-0 based on xVelocity.
        /// </summary>
        [SerializeField]
        private AnimationCurve airDecelerationCurve;

        /// <summary>
        /// Deceleration factor in m/s^2 for when the player is in the air.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float airDecelerationFactor;

        // Ground movement settings section.
        [Header("Ground Movement")]

        /// <summary>
        /// Curve read for the deceleration to be used.
        /// </summary>
        [SerializeField]
        private AnimationCurve groundAccelerationCurve;

        /// <summary>
        /// Acceleration factor in m/s^2 for when the player is grounded.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float groundAccelerationFactor;

        /// <summary>
        /// Curve read for the deceleration to be used.
        /// Decelerations are evaulated in reverse, 1-0 based on xVelocity.
        /// </summary>
        [SerializeField]
        private AnimationCurve groundDecelerationCurve;

        /// <summary>
        /// Deceleration factor in m/s^2 for when the player is grounded.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float groundDecelerationFactor;

        // Debug settings section
        [Header("Debug Settings")]

        // TODO: Debug - Eventually replace with a "debugger" that maintains all debugging state
        /// <summary>
        /// Determines if debugging is enabled for this player controller
        /// </summary>
        [SerializeField]
        private bool debugMode;

        /// <summary>
        /// Player will jump, called by event from player input component.
        /// </summary>
        /// /// <param name="context"></param>
        public void OnJump(InputAction.CallbackContext context)
        {
            // TODO: Jump - Add buffering to jump input.
            jumpPressed = context.ReadValue<float>() > 0;
        }

        /// <summary>
        /// Player will move, direction depending on the input axis's value.
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            xMovementAxis = context.ReadValue<float>();
        }

        // TODO: ensure some types of aiming are prioritized higher than others, seperate "priority aim" bind?
        /// <summary>
        /// Aim vector will be set
        /// </summary>
        /// <param name="context"></param>
        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed) aimAngle = Vector2.SignedAngle(Vector2.right, context.ReadValue<Vector2>());
        }

        /// <summary>
        /// Upon entering a collision
        /// </summary>
        /// <param name="collision">The 2d collision</param>
        public void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateIsGrounded(collision);
        }

        /// <summary>
        /// While in a collision
        /// </summary>
        /// <param name="collision">The 2d collision</param>
        public void OnCollisionStay2D(Collision2D collision)
        {
            UpdateIsGrounded(collision);
        }

        /// <summary>
        /// Start is called first before the game starts.
        /// </summary>
        public void Start()
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// FixedUpdated called by Unity's physics system.
        /// </summary>
        public void FixedUpdate()
        {
            if (debugMode) playerRigidbody.GetComponentInChildren<SpriteRenderer>().color = isGrounded ? Color.green : Color.red;

            AimUpdate();
            MovementUpdate();
            JumpUpdate();

            // We assume that if the player is not moving, then the player will stay in its current state, otherwise its possible to be not grounded
            isGrounded = playerRigidbody.velocity.magnitude <= 0 && isGrounded;
        }

        // HACK: This is a mess that can be simplified
        // TODO: State - Refactor to use a defined a series of states
        /// <summary>
        /// Update the player for one step of time  for movement
        /// Intended to be called in fixed update.
        /// </summary>
        private void MovementUpdate()
        {
            float xVelocity = Mathf.Abs(playerRigidbody.velocity.x);

            // Set the correct acceleration depending on state
            float acceleration = isGrounded ? (groundAccelerationCurve.Evaluate(xVelocity/maxXVelocity)+1)*groundAccelerationFactor : (airAccelerationCurve.Evaluate(xVelocity / maxXVelocity) + 1) *airAccelerationFactor;
            float deceleration = isGrounded ? (groundDecelerationCurve.Evaluate(xVelocity / maxXVelocity)+1)*groundDecelerationFactor : (airDecelerationCurve.Evaluate(xVelocity / maxXVelocity) + 1) *airDecelerationFactor;

            // Calculate the threshold to avoid overshooting auto slow down
            slowdownThreshold = deceleration * Time.fixedDeltaTime;

            // Dot product, 1 being the same direction, 0 being perpendicular, -1 being oposites
            float currentDirection = Vector2.Dot(new Vector2(playerRigidbody.velocity.x,0).normalized, Vector2.right);
            
            // AutoSlowdown, If the player is trying not to move and if the player is grounded proided that autoSlowdownInAir is enabled
            if (xMovementAxis == 0 && (autoSlowdownInAir || isGrounded))
            {
                // If we need the auto slowdown
                if (xVelocity >= slowdownThreshold)
                {
                    playerRigidbody.AddForce(new Vector2(deceleration * playerRigidbody.mass * currentDirection * -1, 0));
                }
                // If we need slowdown and, we should be stopped, stop the player's x movement, to avoid jittering from the slowing
                else if (xVelocity < slowdownThreshold && playerRigidbody.velocity.x != 0)
                { 
                    playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);
                }
            }
            // Deceleration (trying to move opposite to current velocity direction)
            else if (xMovementAxis * -1 == currentDirection)
            {
               playerRigidbody.AddForce(new Vector2(deceleration * playerRigidbody.mass * xMovementAxis, 0));
            }
            // Acceleration, checking if we are overspeed
            else if (xVelocity < maxXVelocity)
            {
                float force = acceleration * playerRigidbody.mass;
                if (force / playerRigidbody.mass * Time.fixedDeltaTime + xVelocity > maxXVelocity)
                {
                    force = ((maxXVelocity - xVelocity) / Time.fixedDeltaTime) * playerRigidbody.mass;
                }

                playerRigidbody.AddForce(new Vector2(force*xMovementAxis, 0));
            }          
        }

        // HACK: This code is a mess, rewrite it all.
        /// <summary>
        /// Update the player for one time step for jumping
        /// Intended to be called in fixed update.
        /// </summary> 
        private void JumpUpdate()
        {
            // Stores if we can jump.
            bool canJump = false;
            // if grounded, update the coyoteTimer
            if (isGrounded && coyoteTimer > 0) coyoteTimer = 0;
            // If not grounded start counting up
            else if (!isGrounded) 
            coyoteTimer += Time.fixedDeltaTime;

            canJump = coyoteTimer <= coyoteTime;

            // If time limit exceded for jumping, or no longer trying to jump
            if (isJumping && (jumpTimer > maxJumpTime || !jumpPressed))
            {
                // Stop jumping
                isJumping = false;
                jumpTimer = 0;
            }
            else if (isJumping) jumpTimer += Time.fixedDeltaTime;
            // When able to jump according to the coyote timer, trying to jump, and not already in a jump
            else if (canJump && jumpPressed)
            {
                isJumping = true;
            }    
                

            // TODO: Jump - Jumps should push the object jumped from with an equal force.
            if (isJumping)
            {
                float force = jumpAccelerationCurve.Evaluate(jumpTimer/maxJumpTime) * jumpAccelerationFactor * playerRigidbody.mass;
                // TODO: Jump - Apply jump force in .normal direction of collided surface?
                playerRigidbody.AddForce(playerUp * force);
            }
        }

        /// <summary>
        /// Updates the rotation of the attack cone to to where the player is aiming.
        /// </summary>
        private void AimUpdate()
        {
            attackCone.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
        }

        /// <summary>
        /// Check if the player is grounded, and update isGrounded to reflect the state
        /// </summary>
        /// <param name="collision">The collision to check if its points are within a valid range</param>
        private void UpdateIsGrounded(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                float relativeAngle = RelativeAngle(contact.point);
                if (relativeAngle <= maxGroundCollisionAngle)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Check the relative angle of the head of the player, to the point provided
        /// </summary>
        /// <param name="point">The point to get the angle of</param>
        /// <returns>Angle of point relative to player head</returns>
        private float RelativeAngle(Vector2 point)
        {
            Vector2 contactDirection = (playerRigidbody.position - point).normalized;
            return Vector3.Angle(contactDirection, playerUp);
        }
    }
}
