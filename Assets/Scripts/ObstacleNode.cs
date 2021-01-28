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
    private Vector3 explosionVector;
    private float aCos;
    private const float InitYMultiplier = 4f;

    public void Initialize(float startLineZ, float finishLineZ, float targetBeat, int trackNumber)
    {
        beat = targetBeat;
        paused = false;
        explosionFired = false;
        obsStartZ = startLineZ;
		obsEndZ = finishLineZ;
		obsStartX = trackNumber > 2 ? -0.25f : 0.25f;
		obsStartY = 0.3f;
		explosionVector = new Vector3(0, -0.3f, 0);
		aCos = Mathf.Cos(targetBeat);

		var initPos = obsStartX * InitYMultiplier;
		transform.position = new Vector3(initPos , obsStartY, obsStartZ);
    }

    private void Update()
    {
        if (Conductor.pauseTimeStamp > 0f) return;

        var dt = aCos * Time.deltaTime * 100;
        transform.Rotate(dt,dt, dt, Space.Self);

        if (paused) return;

        transform.position = new Vector3(Mathf.LerpUnclamped(obsStartX, obsStartX * InitYMultiplier, (beat - Conductor.songposition) / Conductor.appearTime), 
														obsStartY, 
														Mathf.LerpUnclamped(obsEndZ, obsStartZ, (beat - Conductor.songposition) / Conductor.appearTime));

        if (!(Conductor.songposition > beat) || explosionFired) return;
        explosionFired = true;
        Bounce(false);
    }
    
    public void Bounce(bool success)
    {
        StartCoroutine(BounceRoutine(success));
        explosionFired = success;
    }

    private IEnumerator BounceRoutine(bool success)
	{
		yield return new WaitUntil(() => Conductor.songposition >= beat);
		if (success)
        {
            yield return new WaitUntil(() => Conductor.songposition >= beat + 0.1f);
            paused = true;
            var explosionPosition = transform.position + explosionVector;
            rigid.AddExplosionForce(10f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }
        else
        {
            paused = true;
            Handheld.Vibrate();
            var explosionPosition = transform.position + explosionVector;
            rigid.AddExplosionForce(5f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }
		yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
	}
}