using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleNode : MonoBehaviour
{
    public Rigidbody rigid;
    private bool paused;
    private bool explosionFired;
    private float beat;
    private float obsStartX, obsStartY, obsStartZ, obsEndZ;
    private Vector3 explotionVector;
    private float aCos;
    private readonly float initYMultiplier = 4f;
    
    public void Initialize(float startLineZ, float finishLineZ, float targetBeat, int trackNumber)
    {
        beat = targetBeat;
        paused = false;
        explosionFired = false;
        obsStartZ = startLineZ;
		obsEndZ = finishLineZ;
		obsStartX = trackNumber > 2 ? -0.25f : 0.25f;
		obsStartY = 0.3f;
		explotionVector = new Vector3(0, -0.3f, 0);
		aCos = Mathf.Cos(targetBeat);

		float initPos = obsStartX * initYMultiplier;
		transform.position = new Vector3(initPos , obsStartY, obsStartZ);
    }

    void Update()
    {
        if (Conductor.pauseTimeStamp > 0f) return;
        
        transform.Rotate(aCos,aCos,aCos, Space.Self);

        if (paused) return;

        transform.position = new Vector3(Mathf.Lerp(obsStartX, obsStartX * initYMultiplier, (beat - Conductor.songposition) / Conductor.appearTime), 
														obsStartY, 
														Mathf.LerpUnclamped(obsEndZ, obsStartZ, (beat - Conductor.songposition) / Conductor.appearTime));

        if (Conductor.songposition > beat && !explosionFired)
		{
            explosionFired = true;
			Bounce(false);
		}
    }
    
    public void Bounce(bool success)
    {
        StartCoroutine(BounceRoutine(success));
        this.explosionFired = success;
    }

    IEnumerator BounceRoutine(bool success)
	{
		yield return new WaitUntil(() => Conductor.songposition >= beat);
		
        if (success)
        {
            yield return new WaitUntil(() => Conductor.songposition >= beat + 0.1f);
            paused = true;
            Vector3 explosionPosition = transform.position + explotionVector;
            rigid.AddExplosionForce(10f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }
        else
        {
            paused = true;
            Vector3 explosionPosition = transform.position + explotionVector;
            rigid.AddExplosionForce(5f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
        
        Destroy(gameObject);
	}
}
