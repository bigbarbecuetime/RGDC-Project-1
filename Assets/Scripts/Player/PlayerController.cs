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
        // TODO: Ensure Player Controller feels good to use during gameplay.
        // TODO: Seperate Camera controller into its own class
        // TODO: Player gets stuck against walls when moving into them, this may require the method of moving the player to be redesigned.

        /// <summary>
        /// The minimum value that can be entered in unity's inspector for the script
        /// </summary>
        private const int MIN_ATTRIBUTE_VALUE = 0;

        /// <summary>
        /// Value used to determine if moving left or right in the x axis.
        /// </summary>
        private float xMovementAxis = 0;

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
        /// The force the player will experience when jumping.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float jumpForce;

        /// <summary>
        /// Maximum velocity player can have.
        /// </summary>
        [SerializeField]
        [Min(MIN_ATTRIBUTE_VALUE)]
        private float maxVelocity;

        /// <summary>
        /// Acceleration in m/s^2
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float acceleration;

        /// <summary>
        /// Deceleration in m/s^2
        /// </summary>
        [Min(MIN_ATTRIBUTE_VALUE)]
        public float deceleration;

        /// <summary>
        /// Player will jump, called by event from player input component.
        /// </summary>
        public void OnJump()
        {
            // TODO: Only allow jumps when touching (Or very close) to the ground.
            // TODO: Jump height should be variable depending on how long you hold jump
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Player will move, direction depending on the input axis's value.
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            xMovementAxis = context.ReadValue<float>();
        }
        float debugVel = 0;

        public void Update()
        {
            // Draw the current velocity
            Debug.DrawRay(playerRigidbody.position, (playerRigidbody.velocity / maxVelocity) * Camera.main.orthographicSize);
            Debug.DrawRay(playerRigidbody.position + new Vector2(0, 1.5f), (new Vector2(debugVel,0) / maxVelocity) * Camera.main.orthographicSize);
        }

        /// <summary>
        /// FixedUpdated called by Unity's physics system.
        /// </summary>
        public void FixedUpdate()
        {
            // Move player depending on x axis input
            // TODO: This is still WIP, need to cap the change in velocity, while allowing for objects to hit the player with uncapped delta v
            // HACK: Ideally, the input speed should change between states, meaning when in air, you are only allowed to +-2m/s of force, while landed could be +-10m/s
            // Note when moving the player, it is better to use the rigidbody's position, not the transforms
            // Potentially track a "Speed effect" velocity, then once forces reach the maximum speed effect, stop applying them
            // TODO: update to use acceleration
            // TODO update to calculate velocity effect and limit max velocity effect

            //DebugMovement();
            PhysMovement();
        }

        /// <summary>
        /// Value used as a timer for debugging 
        /// </summary>
        private float debugTimer = 0;
        /// <summary>
        /// Used to verify acceleration and velocity values used in the velocity effect section
        /// </summary>
        private void DebugMovement()
        {
            playerRigidbody.AddForce(new Vector2(acceleration * playerRigidbody.mass, 0));
            debugTimer += Time.fixedDeltaTime;

            if (debugTimer * acceleration >= maxVelocity)
            {
                Debug.Log("Time to max velocity: " + debugTimer + " seconds");
                Debug.Log("Calculated velocity: " + debugTimer * acceleration);
                Debug.Log("Actual velocity: " + playerRigidbody.velocity);
                Time.timeScale = 0;
            }
        }
       
        private void PhysMovement()
        {
            // Deceleration (If trying to move opposite to current velocity direction)
            if (xMovementAxis * -1 == Mathf.Ceil(playerRigidbody.velocity.normalized.x))
            {
                playerRigidbody.AddForce(new Vector2(deceleration * playerRigidbody.mass * xMovementAxis, 0));
            }
            // Acceleration, checking if we are overspeed
            else if (Mathf.Abs(playerRigidbody.velocity.x) < maxVelocity)
            {
                playerRigidbody.AddForce(new Vector2(acceleration * playerRigidbody.mass * xMovementAxis, 0));
            }

            // TODO: Add slowdown when no input
        }
    }
}
