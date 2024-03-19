using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A basic countdown for the player's timer.
/// </summary>
public class PlayerTimer : MonoBehaviour
{
   
    public float startingTime = 100.0f;
    private static float currentTime;
    private float timeToNextSecond = 0;
    public static float CurrentTime
    {
        get
        {
            return currentTime;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        currentTime = startingTime;
    }

    // Update is called once per frame
    void Update()
    {
        timeToNextSecond += Time.deltaTime;
        if (timeToNextSecond > 1)
        {
            timeToNextSecond = 0;
            currentTime -= 1;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Time left: {ToString()}");
        GUILayout.EndHorizontal();
    }

    public override string ToString()
    {
        return $"{(int)(currentTime/60)%60}:{currentTime%60}";
    }
}
