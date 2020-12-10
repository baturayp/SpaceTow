using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public GameObject spaceMan;
    public Animator spaceManAnim;

    Coroutine attackRoutine;

    //animation frame values
    private float[] attackStart = {0.16f, 0.2f};
    private float[] attackSuccess = {0.4f, 0.6f};
    private float[] attackFailed = {0.8f, 1f};
    private float attackSpeed = 0.25f;
    private float animVal;

    //temporary attack anim selector
    private int animNum = 1;
    private bool successState = true;
    private float speedVal = 0.1f;
    private string debugLog ;
    private bool manual = false;
    
    void Start()
    {
        animVal = 0f;
        Conductor.KeyDownEvent += KeyDownAction;
    }

    void OnDestroy()
    {
        Conductor.KeyDownEvent -= KeyDownAction;
    }

    void OnGUI() 
    {
        GUI.contentColor = Color.red;
        if (GUI.Button(new Rect(10, 10, 130, 25), "Toggle auto mode")) manual = !manual;
        GUI.Label(new Rect (10, 35, 400, 20), debugLog);
        if (manual) GUI.Label(new Rect (10, 55, 400, 20), "  N-M          J-K             I-O");
    }

    void Update()
    {
        if (manual)
        {
            if (Input.GetKeyDown(KeyCode.N) && animNum > 1)
            {
                animNum--;
            }
            if (Input.GetKeyDown(KeyCode.M) && animNum < 5)
            {
                animNum++;
            }
            if (Input.GetKeyDown(KeyCode.J) && speedVal >= 0.2f)
            {
                speedVal = speedVal - 0.1f;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                speedVal = speedVal + 0.1f;
            }
            if (Input.GetKeyDown(KeyCode.I)) successState = false;
            if (Input.GetKeyDown(KeyCode.O)) successState = true;
            debugLog = "Clip: " + animNum.ToString() + "| Speed: " + speedVal.ToString() + (successState ? "| Hit" : "| Miss");
        }
        if (!manual) debugLog = "Auto Mode";
    }

    void KeyDownAction(int trackNumber, Conductor.Rank rank)
    {
        if (manual) ManualAction(trackNumber, rank);
        //auto
        else
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
    }

    void ManualAction(int trackNumber, Conductor.Rank rank)
    {
        if (rank == Conductor.Rank.MISS) return;
        if (!successState)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackAnim(speedVal, speedVal, trackNumber, false, animNum));
        }
        if (successState)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackAnim(speedVal, speedVal, trackNumber, true, animNum));
        }
    }

    IEnumerator AttackAnim(float hitDuration, float backDuration, int trackNumber, bool success, int animNum)
    {
        float elapsedTime = 0.0f;
        string animToPlay = animNum.ToString() + trackNumber.ToString();
        while (elapsedTime < hitDuration)
        {
            elapsedTime += Time.deltaTime;
            animVal = Mathf.Lerp(attackStart[0], attackStart[1], elapsedTime / hitDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }
        elapsedTime = 0.0f;
        while (elapsedTime < backDuration)
        {
            elapsedTime += Time.deltaTime;
            animVal = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], 
                                   success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
            spaceManAnim.Play(animToPlay, 0, animVal);
            spaceManAnim.Update(0f);
            yield return null;
        }
        animVal = 0f;
        attackRoutine = null;
        spaceManAnim.Play("idle");
    }
}
