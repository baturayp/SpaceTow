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
    
    public void Punch(int trackNumber, int animNumber, float targetBeat)
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackAnim(targetBeat, Conductor.songposition, 0.15f, animNumber, trackNumber));
    }
    
    public void Jump(float targetBeat)
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackAnim(targetBeat, Conductor.songposition, 0.2f, 13, 0));
    }

    IEnumerator AttackAnim(float targetBeat, float punchStarted, float backDuration, int animNum, int trackNumber)
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
        
        float elapsedTime = 0.0f;
        while (elapsedTime < backDuration)
        {
            elapsedTime += Time.deltaTime;
            var animVal = Mathf.Lerp(attackSuccess[0], attackSuccess[1], elapsedTime / backDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }
        attackRoutine = null;
        spaceManAnim.speed = 1f;
        spaceManAnim.Play("idle");
    }
}
