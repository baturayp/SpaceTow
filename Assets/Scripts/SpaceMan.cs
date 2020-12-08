using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public GameObject spaceMan;
    public Animator spaceManAnim;
    private bool animating;
    Coroutine attackAnim;

    //animation frame values
    private float[] attackStart = {0.16f, 0.2f};
    private float[] attackSuccess = {0.4f, 0.6f};
    private float[] attackFailed = {0.8f, 1f};

    //temporary attack anim selector
    private int animNum = 1;
    private bool successState = true;
    private float speedVal = 0.1f;
    private string debugLog = "NO: 1";
    
    void Start()
    {
        Conductor.KeyDownEvent += KeyDownAction;
    }

    void OnDestroy()
    {
        Conductor.KeyDownEvent -= KeyDownAction;
    }

    void OnGUI() 
    {
        GUI.contentColor = Color.green;
        GUI.Label(new Rect (10, 10, 400, 20), debugLog);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && animNum > 1)
        {
            animNum--;
        }
        if (Input.GetKeyDown(KeyCode.M) && animNum < 7)
        {
            animNum++;
        }
        if (Input.GetKeyDown(KeyCode.J) && speedVal > 0.1)
        {
            speedVal = speedVal - 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            speedVal = speedVal + 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.I)) successState = false;
        if (Input.GetKeyDown(KeyCode.O)) successState = true;
        debugLog = "Clip: " + animNum.ToString() + " Speed: " + speedVal.ToString() + (successState ? " Hit" : " Miss");
    }

    void KeyDownAction(int trackNumber, Conductor.Rank rank)
    {
        //if miss event triggered, do nothing (no player input)
        if (rank == Conductor.Rank.MISS)
        {
            return;
        }
        //player inputted down but missed the note, play failed animation
        //else if (rank == Conductor.Rank.WASTE) 
        if (!successState)
        {
            if (attackAnim != null) StopCoroutine(attackAnim);
            attackAnim = StartCoroutine(AttackAnim(speedVal, speedVal, trackNumber, false, animNum));
        }
        //succesful hit
        if (successState)
        {
            if (attackAnim != null) StopCoroutine(attackAnim);
            attackAnim = StartCoroutine(AttackAnim(speedVal, speedVal, trackNumber, true, animNum));
        }
    }

    IEnumerator AttackAnim(float hitTime, float backTime, int trackNumber, bool success, int animNo)
    {
        float elapsedTime = 0.0f;
        animating = true;
        string animToPlay = animNo.ToString() + trackNumber.ToString();
        while (elapsedTime < hitTime)
        {
            elapsedTime += Time.deltaTime;
            float val = Mathf.Lerp(attackStart[0], attackStart[1], elapsedTime / hitTime);
            spaceManAnim.Play(animToPlay, 0, val);
            spaceManAnim.Update(0f);
            yield return null;
        }
        elapsedTime = 0.0f;
        while (elapsedTime < backTime)
        {
            elapsedTime += Time.deltaTime;
            float val = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], 
                                   success? attackSuccess[1] : attackFailed[1], elapsedTime / backTime);
            spaceManAnim.Play(animToPlay, 0, val);
            spaceManAnim.Update(0f);
            yield return null;
        }
        animating = false;
        spaceManAnim.Play("idle");
    }
}
