using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks statistics of the player, such as speed, attack dmg, etc.
/// </summary>
public static class StatsTracker 
{
    private static float speed = 1.0f;
    private static float attack = 1.0f;


    /// <summary>
    /// Players speed.
    /// Cannot be set to less than 1.
    /// </summary>
    public static float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            if (value < 1)
            {
                value = 1;
            }
            speed = value;
        }
    }


    /// <summary>
    /// Players attack damage.
    /// Cannot be set to less than 1.
    /// </summary>
    public static float Attack
    {
        get
        {
            return attack;
        }
        set
        {
            if (value < 1)
            {
                value = 1;
            }
            attack = value;
        }
    }
}
