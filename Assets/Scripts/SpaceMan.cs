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
    }

    void OnDestroy()
    {
        PlayerInputControl.InputtedEvent -= PlayerInputted;
    }

    void PlayerInputted(int trackNumber)
    {
        //car
    }
}
