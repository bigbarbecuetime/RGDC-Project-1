using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGDCP1.Debugging
{
    public class Debugger : MonoBehaviour
    {
        /// <summary>
        /// Original timescale of the project.
        /// </summary>
        private float initialTimescale;

        /// <summary>
        /// Original gravity of the physics system.
        /// </summary>
        private Vector2 initialGravity;

        /// <summary>
        /// Defines if frameRate debugging is enabled or not.
        /// </summary>
        public bool frameRateDebugging = false;

        /// <summary>
        /// The target framerate, -1 being default.
        /// </summary>
        [Min(-1)]
        public int targetFrameRate = -1;

        /// <summary>
        /// Defines if timeScale debugging is enabled or not.
        /// </summary>
        public bool timeScaleDebugging = false;

        /// <summary>
        /// Time scale the project will be set to.
        /// </summary>
        [Min(0)]
        public float timeScale;

        /// <summary>
        /// Defines if gravity debugging is enabled or not.
        /// </summary>
        public bool gravityDebugging = false;

        /// <summary>
        /// Time scale the project will be set to.
        /// </summary>
        public Vector2 gravity;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            initialTimescale = Time.timeScale;
            initialGravity = Physics2D.gravity;
        }

        // Update is called once per frame
        void Update()
        {
            Application.targetFrameRate = frameRateDebugging ? targetFrameRate : -1;
            Time.timeScale = timeScaleDebugging ? timeScale : initialTimescale;
            Physics2D.gravity = gravityDebugging ? gravity : initialGravity;
        }
    }
}