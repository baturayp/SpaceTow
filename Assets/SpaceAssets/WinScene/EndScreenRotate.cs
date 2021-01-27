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
    public bool win;

    // Update is called once per frame

    void Start()
    {
        if (Screen.orientation == ScreenOrientation.LandscapeLeft | Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 90);
            bkgTape.localPosition = new Vector3(225, -251.5f, 0);
            icon1.localPosition = new Vector3(217, -110, 0);
            icon2.localPosition = new Vector3(217, 110, 0);
            if (!win)
            {
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

        if (Screen.orientation == ScreenOrientation.Portrait | Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 0);
            bkgTape.localPosition = new Vector3(0, -251.5f, 0);
            icon1.localPosition = new Vector3(110, -246, 0);
            icon2.localPosition = new Vector3(-110, -246, 0);
            if (!win)
            {
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
    void Update()
    {
        if (oldOrientation == 2 && (Screen.orientation == ScreenOrientation.LandscapeLeft | Screen.orientation == ScreenOrientation.LandscapeRight))
        {
            orientationChange = true;
        }
        if (oldOrientation == 1 && (Screen.orientation == ScreenOrientation.Portrait | Screen.orientation == ScreenOrientation.PortraitUpsideDown))
        {
            orientationChange = true;
        }

        if (orientationChange == true && (Screen.orientation == ScreenOrientation.LandscapeLeft | Screen.orientation == ScreenOrientation.LandscapeRight))
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 90);
            bkgTape.localPosition = new Vector3(225, -251.5f, 0);
            icon1.localPosition = new Vector3(217, -110, 0);
            icon2.localPosition = new Vector3(217, 110, 0);
            if (win)
            {
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

        if (orientationChange == true && (Screen.orientation == ScreenOrientation.Portrait | Screen.orientation == ScreenOrientation.PortraitUpsideDown))
        {
            bkgTape.localRotation = Quaternion.Euler(0, 0, 0);
            bkgTape.localPosition = new Vector3(0, -251.5f, 0);
            icon1.localPosition = new Vector3(110, -246, 0);
            icon2.localPosition = new Vector3(-110, -246, 0);
            if (!win)
            {
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
