using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RGDCP1.Player
{
    /// <summary>
    /// PlayerController is used to determine the physical movements of the player in the world.
    /// It is a physics based controller using a rigidbody.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        // TODO: Player Controller will eventually use a Finite State Machine to track its current state. (Ex. Falling, Running)
        // TODO: Add support for acceleration "functions", exponential, logarithmic, linear ans so on.

        /// <summary>
        /// The minimum value that can be entered in unity's inspector for the script.
        /// </summary>
        private const int MIN_ATTRIBUTE_VALUE = 0;

        /// <summary>
        /// Value used to determine if moving left or right in the x axis.
        /// </summary>
        private float xMovementAxis = 0;

        /// <summary>
        /// Threashold to see when we need to slow down the player automatically, when landed and no input is pressed, calculated based on deceleration.
        /// </summary>
        private float slowdownThreshold;

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
        public float jumpAcceleration;

        /// <summary>
        /// Maximum velocity player can have.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float maxVelocity;

        /// <summary>
        /// Acceleration in m/s^2 for when the player is in the air.
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float airAcceleration;

        /// <summary>
        /// Deceleration in m/s^2 for when the player is in the air.
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float airDeceleration;

        /// <summary>
        /// Acceleration in m/s^2 for when the player is grounded.
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float groundAcceleration;

        /// <summary>
        /// Deceleration in m/s^2 for when the player is grounded.
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float groundDeceleration;

        // TODO: Comment
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float maxJumpAngle;

        // Debug settings section
        [Header("Debug Settings")]
        [SerializeField]

        // TODO: Eventually replace with a "debugger" that maintains all debugging state
        /// <summary>
        /// Determines if debugging is enabled for this player controller
        /// </summary>
        private bool debugMode;

                    
        private bool isGrounded = false;

        // TODO: Jumps should occur in their own function, fired in fixed update.
        /// <summary>
        /// Player will jump, called by event from player input component.
        /// </summary>
        /// /// <param name="context"></param>
        public void OnJump(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();
            // TODO: Apply jump force in .normal direction of collided surface.
            // TODO: Only allow jumps when touching (Or very close) to the ground (Buffered).
            // TODO: Jump height should be variable depending on how long you hold jump
            // TODO: Jumps should push the object jumped from in a proper way
            if (value > 0 && isGrounded) playerRigidbody.AddForce(Vector3.up * jumpAcceleration * value * playerRigidbody.mass, ForceMode2D.Impulse);
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
        /// 
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateIsGrounded(collision);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
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
            // Move player depending on x axis input
            // Note when moving the player, it is better to use the rigidbody's position, not the transforms
            PhysMovementUpdate();
            
            // We assume that if the player is not moving, then the player will stay in its current state, otherwise its possible to be not grounded
            isGrounded = playerRigidbody.velocity.magnitude <= 0 ? isGrounded : false;
        }

        // TODO: Refactor to use and define a series of states
        /// <summary>
        /// Update the player for one step of time, based on phyisics
        /// </summary>
        private void PhysMovementUpdate()
        {
            // Currently exponential
            float acceleration = groundAcceleration*groundAcceleration;
            float deceleration = groundDeceleration*groundDeceleration;

            // Calculate the threshold to avoid overshooting auto slow down
            slowdownThreshold = deceleration * Time.fixedDeltaTime;

            // Dot product, 1 being the same direction, 0 being perpendicular, -1 being oposites
            float currentDirection = Vector2.Dot(new Vector2(playerRigidbody.velocity.x,0).normalized, Vector2.right);
            
            if (xMovementAxis == 0)
            {
                // TODO: should check if in air, only want this when grounded
                if (Mathf.Abs(playerRigidbody.velocity.x) >= slowdownThreshold)
                {
                    playerRigidbody.AddForce(new Vector2(deceleration * playerRigidbody.mass * currentDirection * -1, 0));
                }
                // Ensures if we are under the threashold, we should be stopped, avoids jittering
                else if (Mathf.Abs(playerRigidbody.velocity.x) < slowdownThreshold && playerRigidbody.velocity.x != 0)
                { 
                    playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);
                }
            }
            // Deceleration (If trying to move opposite to current velocity direction)
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
        /// Check if the player is grounded, and update isGrounded to reflect the state
        /// </summary>
        /// <param name="collision">The collision to check if its points are within a valid range</param>
        private void UpdateIsGrounded(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                float relativeAngle = RelativeAngle(contact.point);
                if (relativeAngle <= maxJumpAngle)
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
            Vector2 playerHead = Vector3.up;
            return Vector3.Angle(contactDirection, playerHead);
        }
    }
}
