using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    public ParticleSystem[] particles;
    private bool paused;

    private void Update()
    {
        if (Conductor.paused)
        {
            if (!paused) paused = true; ScenePaused();
        }
        else
        {
            if (paused) paused = false; ScenePaused();
        }
    }

    private void ScenePaused()
    {
        switch (paused)
        {
            case true:
            {
                var len = particles.Length;
                for (var i = 0; i < len; i++)
                {
                    particles[i].Pause();
                }
                break;
            }
            case false:
            {
                var len = particles.Length;
                for (var i = 0; i < len; i++)
                {
                    particles[i].Play();
                }
                break;
            }
        }
    }
}