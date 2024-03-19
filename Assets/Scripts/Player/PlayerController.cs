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

        // TODO: Player gets stuck against walls when moving into them, this may require the method of moving the player to be redesigned.

        /// <summary>
        /// The minimum value that can be entered in unity's inspector for the script
        /// </summary>
        private const int MIN_ATTRIBUTE_VALUE = 1;

        /// <summary>
        /// Value used to determine if moving left or right in the x axis.
        /// </summary>
        private float xMovementAxis = 0;

        /// <summary>
        /// Rigidbody used for the player's movement.
        /// </summary>
        [SerializeField]
        private Rigidbody2D playerRigidbody;

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
        /// Player will jump, called by event from player input component.
        /// </summary>
        public void OnJump()
        {
            // TODO: Only allow jumps when touching (Or very close) to the ground.
            playerRigidbody.AddForce(Vector3.up * jumpForce);
        }

        /// <summary>
        /// Player will move, direction depending on the input axis's value.
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            // HACK: This design does not seem good, potentially update movement some other way.
            xMovementAxis = context.ReadValue<float>();
        }

        /// <summary>
        /// FixedUpdated called by Unity's physics system.
        /// </summary>
        public void FixedUpdate()
        {
            // Move player depending on x axis input
            playerRigidbody.velocity = new Vector2(xMovementAxis * maxVelocity, playerRigidbody.velocity.y);
        }
    }
}
