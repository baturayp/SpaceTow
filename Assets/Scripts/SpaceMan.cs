using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public Animator spaceManAnim;
    private Coroutine attackRoutine;

    //animation frame values
    private float[] attackStart = {0.16f, 0.2f};
    private float[] attackSuccess = {0.4f, 0.6f};
    private float[] attackFailed = {0.8f, 1f};
    private float attackSpeed = 0.25f;
    
    void Start()
    {
        Conductor.KeyDownEvent += KeyDownAction;
        Conductor.SpaceJumpEvent += Jump;
    }

    void OnDestroy()
    {
        Conductor.KeyDownEvent -= KeyDownAction;
        Conductor.SpaceJumpEvent -= Jump;
    }

    void Update()
    {
        //
    }
    
    //temporary
    public void Jump()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackAnim(attackSpeed, attackSpeed, 2, false, 0));
    }

    void KeyDownAction(int trackNumber, Conductor.Rank rank)
    {
        //if miss event triggered, do nothing (no player input)
        if (rank == Conductor.Rank.MISS) return;
        //player inputted down but failed to hit the note
        if (rank == Conductor.Rank.WASTE)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackAnim(attackSpeed, attackSpeed, trackNumber, false, 3));
        }
        //hit succeed
        if (rank == Conductor.Rank.BAD || rank == Conductor.Rank.GOOD || rank == Conductor.Rank.PERFECT)
        {
            //if animation already running, stop it
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            //upcoming note is close enough, adjust speed accordingly
            attackRoutine = StartCoroutine(AttackAnim(Conductor.dueToNextNote[trackNumber], attackSpeed, trackNumber, true, Conductor.nextNoteAnim[trackNumber]));
        }
    }

    IEnumerator AttackAnim(float hitDuration, float backDuration, int trackNumber, bool success, int animNum)
    {
        float elapsedTime = 0.0f;
        string animToPlay = animNum.ToString() + trackNumber.ToString();
        while (elapsedTime < hitDuration)
        {
            elapsedTime += Time.deltaTime;
            var animVal = Mathf.Lerp(attackStart[0], attackStart[1], elapsedTime / hitDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }
        elapsedTime = 0.0f;
        while (elapsedTime < backDuration)
        {
            elapsedTime += Time.deltaTime;
            var animVal = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], 
                                   success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }
        attackRoutine = null;
        spaceManAnim.Play("idle");
    }
}
