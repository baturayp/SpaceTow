using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
    public GameObject spaceMan;
    public Animator spaceManAnim;
    
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
        spaceManAnim.SetInteger("readyState", Conductor.nextNoteTrack);
    }

    void PlayerInputted(int trackNumber)
    {
        spaceManAnim.SetInteger("punchState", trackNumber == 0 ? -1 : trackNumber);
    }

    void BeatOnHit(int trackNumber, Conductor.Rank rank)
    {
        if (rank != Conductor.Rank.MISS)
        {
            spaceManAnim.SetInteger("punchState", trackNumber == 0 ? -2 : 2);
        }
    }

    public void ResetState()
    {
        spaceManAnim.SetInteger("punchState", 0);
    }
}
