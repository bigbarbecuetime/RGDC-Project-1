using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    /// <summary>
    /// Original timescale of the project.
    /// </summary>
    private float initalTimescale;

    /// <summary>
    /// Defines if debugging is enabled or not.
    /// </summary>
    public bool debugging = false;

    /// <summary>
    /// The target framerate, -1 being default.
    /// </summary>
    [Min(1)]
    public int targetFrameRate = -1;

    /// <summary>
    /// Time scale the project will be set to.
    /// </summary>
    [Min(0)]
    public float timeScale;

    private void Start()
    {
        initalTimescale = Time.timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = debugging ? targetFrameRate : -1;
        Time.timeScale = debugging ? timeScale : initalTimescale;
    }
}
