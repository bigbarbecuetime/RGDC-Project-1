using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGDCP1.Debug
{

    public class RigidbodyDebug : MonoBehaviour
    {
        /// <summary>
        /// Maximum magnatude that will fit within the bounds of the screen
        /// </summary>
        private static float maxVelocityMagnitude = 40;

        /// <summary>
        /// Rigidbody of the gameobject
        /// </summary>
        private Rigidbody rb;

        /// <summary>
        /// Rigidbody2D of the gameobject
        /// </summary>
        private Rigidbody2D rb2d;

        // Start is called before the first frame update
        void Start()
        {
            gameObject.TryGetComponent<Rigidbody>(out rb);
            if (rb == null)
            {
                gameObject.TryGetComponent<Rigidbody2D>(out rb2d);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Draw the current velocity for debugging
            if (rb != null) UnityEngine.Debug.DrawRay(rb.position, (rb.velocity / maxVelocityMagnitude) * Camera.main.orthographicSize);
            else if (rb2d != null) UnityEngine.Debug.DrawRay(rb2d.position, (rb2d.velocity / maxVelocityMagnitude) * Camera.main.orthographicSize);
        }
    }
}
