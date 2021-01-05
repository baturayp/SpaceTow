using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    public ParticleSystem[] particles;
    private bool paused;
    
    void Update()
    {
        if (Conductor.pauseTimeStamp > 0)
        {
            if (!paused) paused = true; ScenePaused();
        }
        else
        {
            if (paused) paused = false; ScenePaused();
        }
    }

    void ScenePaused()
    {
        if (paused)
        {
            int len = particles.Length;
            for (int i = 0; i < len; i++)
            {
                particles[i].Pause();
            }
        }

        if (!paused)
        {
            int len = particles.Length;
            for (int i = 0; i < len; i++)
            {
                particles[i].Play();
            }
        }
    }
}
