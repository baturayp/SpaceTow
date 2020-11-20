using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public GameObject spaceMan;
    public Animator spaceManAnim;
    private bool animating;
    Coroutine motionAnim;
    
    void Start()
    {
        PlayerInputControl.InputtedEvent += PlayerInputted;
        Conductor.BeatOnHitEvent += BeatOnHit;
    }

    void OnDestroy()
    {
        PlayerInputControl.InputtedEvent -= PlayerInputted;
        Conductor.BeatOnHitEvent -= BeatOnHit;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            spaceManAnim.SetLayerWeight(1, 0);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            spaceManAnim.SetLayerWeight(1, 1);
        }
    }

    void PlayerInputted(int trackNumber)
    {
        if (!animating)
        {
            motionAnim = StartCoroutine(Motion(0.2f, 0.2f, 0f,trackNumber == 0 ? -2 : 2));
        }
    }

    void BeatOnHit(int trackNumber, Conductor.Rank rank)
    {
        if (rank != Conductor.Rank.MISS)
        {
            StopCoroutine(motionAnim);
            motionAnim = StartCoroutine(Motion(0.05f, 0.3f, 0f,trackNumber == 0 ? -2 : 2));
        }
    }

    IEnumerator Motion(float hitTime, float backTime, float fromVal, float toVal)
    {
        float elapsedTime = 0.0f;
        animating = true;
        while (elapsedTime < hitTime)
        {
            elapsedTime += Time.deltaTime;
            float val = Mathf.Lerp(fromVal, toVal, elapsedTime / hitTime);
            spaceManAnim.SetFloat("position", val);
            yield return null;
        }
        elapsedTime = 0.0f;
        while (elapsedTime < backTime)
        {
            elapsedTime += Time.deltaTime;
            float val = Mathf.Lerp(toVal, fromVal, elapsedTime / backTime);
            spaceManAnim.SetFloat("position", val);
            yield return null;
        }
        animating = false;
    }
}
