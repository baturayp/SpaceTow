using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public Animator spaceManAnim;
    private Coroutine attackRoutine;

    //animation frame values
    private readonly float[] attackStart = {0.16f, 0.2f};
    private readonly float[] attackSuccess = {0.4f, 0.6f};
    private readonly float[] attackFailed = {0.8f, 1f};
    
    public void Punch(int animNumber, int trackNumber, float targetBeat, bool success)
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        
        //meteor punch anim
        if (trackNumber < 2)
        {
            attackRoutine = StartCoroutine(AttackAnim(targetBeat, Conductor.songposition, 0.25f, animNumber, trackNumber, success));
        }
        //obstacle avoid anim
        else
        {
            //
        }
    }

    IEnumerator AttackAnim(float targetBeat, float punchStarted, float backDuration, int animNum, int trackNumber, bool success)
    {
        spaceManAnim.speed = 0f;
        string animToPlay = animNum.ToString() + trackNumber.ToString();
        while (Conductor.songposition < targetBeat)
        {
            var animVal = Mathf.Lerp(attackStart[0], attackStart[1], (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }

        spaceManAnim.Play(animToPlay, 0 , attackStart[1]);
        
        float elapsedTime = 0.0f;
        while (elapsedTime < backDuration)
        {
            elapsedTime += Time.deltaTime;
            var animVal = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }

        attackRoutine = null;
        spaceManAnim.speed = 1f;
        spaceManAnim.Play("idle");
    }

    IEnumerator AvoidAnim(float targetBeat, float punchStarted, int animNum, int trackNumber, bool success)
    {
        spaceManAnim.speed = 0f;
        int aNum = success ? animNum : 0;
        string animToPlay = aNum.ToString() + trackNumber.ToString();
        
        while (Conductor.songposition < targetBeat)
        {
            var animVal = Mathf.Lerp(0f, 1.0f, (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }

        attackRoutine = null;
        spaceManAnim.speed = 1f;
        spaceManAnim.Play("idle");
    }
}
