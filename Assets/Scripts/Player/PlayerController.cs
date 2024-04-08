using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace RGDCP1.Player
{
    /// <summary>
    /// PlayerController is used to determine the physical movements, and movement state of the player in the world.
    /// It is a physics based controller relying on a rigidbody2d.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        // TODO: State - Player Controller will eventually use a Finite State Machine to track its current state. (Ex. Falling, Running)
        // TODO: Acceleration  Functions - Add support for acceleration "functions", exponential, logarithmic, linear ans so on.
        // TODO: Acceleration Functions - Update to use curves for acceleration, rather than fixed functions

        /// <summary>
        /// The minimum value that can be entered in unity's inspector for the script.
        /// </summary>
        private const int MIN_ATTRIBUTE_VALUE = 0;

        // TODO: Movement - Update to support more than just -1, 0, 1 should work with all floating point values
        /// <summary>
        /// Value used to determine if trying to move left or right in the x axis.
        /// Fixed between -1 - 1
        /// </summary>
        private float xMovementAxis = 0;

        /// <summary>
        ///  Value used to determine if trying to jump
        /// </summary>
        private bool jumpPressed;

        /// <summary>
        /// Threashold to see when we need to slow down the player automatically, when landed and no input is pressed, calculated based on deceleration.
        /// </summary>
        private float slowdownThreshold;

        /// <summary>
        /// Direction of the "head" of the player, its relative up
        /// </summary>
        private Vector2 playerUp = Vector3.up;

        /// <summary>
        /// Rigidbody used for the player's movement.
        /// </summary>
        [SerializeField]
        private Rigidbody2D playerRigidbody;

        // Camera settings section.
        [Header("Camera Settings")]

        // Movement settings section.
        [Header("Movement Settings")]

        /// <summary>
        /// The acceleration the player will experience when jumping.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float jumpAcceleration;

        /// <summary>
        /// Maximum velocity player can have.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxVelocity;

        /// <summary>
        /// Acceleration in m/s^2 for when the player is in the air.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float airAcceleration;

        /// <summary>
        /// Deceleration in m/s^2 for when the player is in the air.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float airDeceleration;

        /// <summary>
        /// Acceleration in m/s^2 for when the player is grounded.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float groundAcceleration;

        /// <summary>
        /// Deceleration in m/s^2 for when the player is grounded.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float groundDeceleration;

        /// <summary>
        /// Should the player auto slowdown when in the air?
        /// </summary>
        [SerializeField]
        private bool autoSlowdownInAir;

        /// <summary>
        /// Maximum angle allowed to be considered if the player is "touching the ground" upon collision
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxGroundCollisionAngle;

        // Debug settings section
        [Header("Debug Settings")]

        // TODO: Debug - Eventually replace with a "debugger" that maintains all debugging state
        /// <summary>
        /// Determines if debugging is enabled for this player controller
        /// </summary>
        [SerializeField]
        private bool debugMode;

        // TODO: State - state should be stored in a seperate state machine class
        /// <summary>
        /// Stores if the player is currently grounded or not
        /// The player is considered grounded when, the player's "feet" are touching a surface below it
        /// </summary>
        private bool isGrounded = false;

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
        /// FixedUpdated called by Unity's physics system.
        /// </summary>
        public void FixedUpdate()
        {
            if (debugMode) playerRigidbody.GetComponent<SpriteRenderer>().color = isGrounded ? Color.green : Color.red;
            
            MovementUpdate();
            JumpUpdate();
            
            // We assume that if the player is not moving, then the player will stay in its current state, otherwise its possible to be not grounded
            isGrounded = playerRigidbody.velocity.magnitude <= 0 ? isGrounded : false;
        }

        // TODO: State - Refactor to use a defined a series of states
        /// <summary>
        /// Update the player for one step of time  for movement
        /// </summary>
        private void MovementUpdate()
        {
            // Check if the acceleration should
            // Currently exponential
            // TODO: Acceleration Functions - should use functions rather than fixed
            float acceleration = isGrounded ? groundAcceleration*groundAcceleration : airAcceleration*airAcceleration;
            float deceleration = isGrounded ? groundDeceleration*groundDeceleration : airDeceleration*airDeceleration;

            // Calculate the threshold to avoid overshooting auto slow down
            slowdownThreshold = deceleration * Time.fixedDeltaTime;

            // Dot product, 1 being the same direction, 0 being perpendicular, -1 being oposites
            float currentDirection = Vector2.Dot(new Vector2(playerRigidbody.velocity.x,0).normalized, Vector2.right);
            
            // AutoSlowdown, If the player is trying not to move and if the player is grounded proided that autoSlowdownInAir is enabled
            if (xMovementAxis == 0 && (autoSlowdownInAir || isGrounded))
            {
                // If we need the auto slowdown
                if (Mathf.Abs(playerRigidbody.velocity.x) >= slowdownThreshold)
                {
                    playerRigidbody.AddForce(new Vector2(deceleration * playerRigidbody.mass * currentDirection * -1, 0));
                }
                // If we need slowdown and, we should be stopped, stop the player's x movement, to avoid jittering from the slowing
                else if (Mathf.Abs(playerRigidbody.velocity.x) < slowdownThreshold && playerRigidbody.velocity.x != 0)
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
            else if (Mathf.Abs(playerRigidbody.velocity.x) < maxVelocity)
            {
                float force = acceleration * playerRigidbody.mass;
                if (force / playerRigidbody.mass * Time.fixedDeltaTime + Mathf.Abs(playerRigidbody.velocity.x) > maxVelocity)
                {
                    force = ((maxVelocity - Mathf.Abs(playerRigidbody.velocity.x)) / Time.fixedDeltaTime) * playerRigidbody.mass;
                }
                playerRigidbody.AddForce(new Vector2(force*xMovementAxis, 0));
            }          
        }
         
        /// <summary>
        /// Update the player for one time step for jumping
        /// </summary> 
        private void JumpUpdate()
        {
            // TODO: Jump - Apply jump force in .normal direction of collided surface.
            // TODO: Jump - Jumps should be restricted, by time? height?.
            // TODO: Jump - Jumps should push the object jumped from in a proper way.
            if (jumpPressed) playerRigidbody.AddForce(playerUp * jumpAcceleration * playerRigidbody.mass);
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
