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

        /// <summary>
        /// Rigidbody used for the player's movement.
        /// </summary>
        [SerializeField]
        private Rigidbody2D playerRigidbody;

        /// <summary>
        /// The force the player will experience when jumping.
        /// </summary>
        [SerializeField]
        private float jumpForce = 0f;

        /// <summary>
        /// Maximum velocity player can have.
        /// </summary>
        [SerializeField]
        private float maxVelocity = 0f;

        /// <summary>
        /// Player will jump, called by event from player input component.
        /// </summary>
        void OnJump()
        {
            // TODO: Only allow jumps when touching (Or very close) to the ground.
            playerRigidbody.AddForce(Vector3.up * jumpForce);
        }

        /// <summary>
        /// Player will move, direction depending on the input axis's value.
        /// </summary>
        private void OnMove(InputAction.CallbackContext context)
        {
            // TODO: 
            playerRigidbody.velocity = context.ReadValue<Vector3>() * maxVelocity;
        }
    }
}
