using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreenRotate : MonoBehaviour
{
    public RectTransform bkgTape;
    public RectTransform icon1;
    public RectTransform icon2;
    bool orientationChange = true;
    int oldOrientation = 0;
    public Animator spaceman;
    public Animator bkg;

    // Update is called once per frame
    void Update()
    {
        if (oldOrientation == 2 & Input.deviceOrientation == DeviceOrientation.LandscapeLeft | Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            orientationChange = true;
        }
        if (oldOrientation == 1 & Input.deviceOrientation == DeviceOrientation.Portrait)
        {
            orientationChange = true;
        }

        if (orientationChange == true & Input.deviceOrientation == DeviceOrientation.LandscapeLeft | Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 90);
            bkgTape.localPosition = new Vector3(225, -251.5f, 0);
            icon1.localPosition = new Vector3(217, 0, 0);
            if (icon2 != null)
            {
                icon2.localPosition = new Vector3(217, 0, 0);
                spaceman.Play("LoseLandImg", 0, 0);
                bkg.Play("BkgAnimLose", 0, 0);
            }
            else
            {
                spaceman.Play("WinLandImg", 0, 0);
                bkg.Play("BkgAnim", 0, 0);
            }

            orientationChange = false;
            oldOrientation = 1;
        }
        if (orientationChange == true & Input.deviceOrientation == DeviceOrientation.Portrait)
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 0);
            bkgTape.localPosition = new Vector3(0, -251.5f, 0);
            icon1.localPosition = new Vector3(0, -246, 0);
            if (icon2 != null)
            {
                icon2.localPosition = new Vector3(0, -246, 0);
                spaceman.Play("LosePortImg", 0, 0);
                bkg.Play("BkgAnimLose", 0, 0);
            }
            else
            {
                spaceman.Play("WinPortImg", 0, 0);
                bkg.Play("BkgAnim", 0, 0);
            }

            orientationChange = false;
            oldOrientation = 2;
        }
    }
}
