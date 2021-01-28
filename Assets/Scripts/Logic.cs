using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Logic : MonoBehaviour
{
    private const float MAXSwipeTime = 0.5f;
    private const float MINSwipeDistance = 0.10f;
    private Vector2 startPos;
    private float startTime;
    private bool songChanged;
    public AudioSource loopPlayer;

    //stars
    public MeshRenderer staticStars;
    private Material starsMat;
    private Coroutine routine, fadeRoutine, songChangeRoutine;

    //camera selection
    public GameObject[] cams;
    public GameObject[] planets;
    public AudioClip[] songloops;
    public Image[] uiElements;
    public Text[] texts;
    private int len, cur;
    public Color[] colors;
    public Color[] uiColors;
    public Image leftButton, rightButton, songInfoFrame, songBkg, playBtn;
    public Sprite playIcon, lockedIcon;
    public Toggle effectsToggle;
    public Text songTitle, artistName;
    public Image fadeLayer;
    public Animator lockAnimator;
    private int lastColor;
    private int lastLevel;
    private bool touchMoving;
    private readonly string[] scenes = { "Chapel", "Beach", "Barn", "Toxic", "Station" };
    private readonly string[] tracks = { "Twelve Days", "Fright Night Twist", "Born Barnstormers", "Run!", "Mystica" };
    private readonly string[] artists = { "Alexander Nakarada", "Bryan Teoh", "Brian Boyko", "Komiku", "Alexander Nakarada" };
    private static readonly int NoiseColor = Shader.PropertyToID("_NoiseColor");
    private static readonly int MainColor = Shader.PropertyToID("_MainColor");

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeRoutine());
    }

    private void Start()
    {
        len = cams.Length;
        lastLevel = PlayerPrefs.GetInt("lastLevel", 1);
        lastColor = cur = lastLevel - 1;
        starsMat = staticStars.material;
        starsMat.SetColor(NoiseColor, colors[0]);
        loopPlayer.clip = songloops[cur];
        loopPlayer.Play();
        SetCurrent(cur);


        for (int i = 0; i < 5; i++)
        {
            if (i > cur)
            {
                foreach (Transform child in planets[i].transform)
                {
                    if (child.GetComponent<MeshRenderer>())
                    {
                        var cl = new Color(0.1f, 0.1f, 0.1f, 1);
                        child.GetComponent<MeshRenderer>().material.SetColor(MainColor, cl);
                    }
                }
            }
        }

        //get effects pref
        effectsToggle.isOn = PlayerPrefs.GetInt("punchEffects", 1) == 1;
    }

    public void Update()
    {
        if (Input.touches.Length <= 0) return;
        if (SongPickingControl._settingsIsActive) return;

        var t = Input.GetTouch(0);

        switch (t.phase)
        {
            case TouchPhase.Began:
                startPos = new Vector2(t.position.x / Screen.width, t.position.y / Screen.width);
                startTime = Time.time;
                break;
            case TouchPhase.Moved:
                touchMoving = true;
                break;
            case TouchPhase.Ended when Time.time - startTime > MAXSwipeTime:
                return;
            case TouchPhase.Ended:
                {
                    touchMoving = false;
                    var endPos = new Vector2(t.position.x / Screen.width, t.position.y / Screen.width);
                    var swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);
                    if (swipe.magnitude < MINSwipeDistance) // Too short swipe
                        return;
                    if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    { // Horizontal swipe
                        if (swipe.x > 0) SwipedRight();
                        else SwipedLeft();
                    }
                    break;
                }
        }
    }

    public void SwipedRight()
    {
        lastColor = cur;
        for (var i = 0; i < len; i++)
            cams[i].SetActive(false);
        cur = --cur % len;
        if (cur < 0) cur = len - 1;
        cams[cur].SetActive(true);

        if (routine != null)
        {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(SetColor(lastColor, cur));


        if (songChangeRoutine != null)
        {
            StopCoroutine(songChangeRoutine);
        }
        songChangeRoutine = StartCoroutine(SetLoop(cur, songChanged));


        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(UIFadeIn(cur));
        }
        else fadeRoutine = StartCoroutine(UIFadeOut(lastColor));
    }

    public void SwipedLeft()
    {
        lastColor = cur;
        for (var i = 0; i < len; i++)
            cams[i].SetActive(false);
        cur = ++cur % len;
        cams[cur].SetActive(true);

        if (routine != null)
        {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(SetColor(lastColor, cur));

        if (songChangeRoutine != null)
        {
            StopCoroutine(songChangeRoutine);
        }
        songChangeRoutine = StartCoroutine(SetLoop(cur, songChanged));

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(UIFadeIn(cur));
        }
        else fadeRoutine = StartCoroutine(UIFadeOut(lastColor));
    }

    public void LevelSelect()
    {
        if (cur < lastLevel)
        {
            StartCoroutine(ButtonSelectRoutine());
        }
        else lockAnimator.Play("lockIcon", 0);
    }

    private IEnumerator ButtonSelectRoutine()
    {
        yield return new WaitForSeconds(0.07f);
        if (!touchMoving) StartCoroutine(LevelSelectRoutine());
    }

    public void EffectsToggle(bool tg)
    {
        PlayerPrefs.SetInt("punchEffects", tg ? 1 : 0);
    }

    private void SetCurrent(int cr)
    {
        var color = colors[cr];
        starsMat.SetColor(NoiseColor, color);
        SetUIColors(uiColors[cr]);
        for (int i = 0; i < len; i++)
            cams[i].SetActive(false);
        cams[cr].SetActive(true);
        songTitle.text = tracks[cr];
        artistName.text = "by " + artists[cr];
        rightButton.color =
                leftButton.color =
                songInfoFrame.color =
                playBtn.color =
                new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 1f);
        songBkg.color = new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 0.3f);
        songTitle.color = artistName.color = new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 1f);
        foreach (var text in texts)
        {
            text.color = uiColors[cr];
        }
    }

    private IEnumerator SetColor(int from, int to)
    {
        var elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            //set skybox color
            var r = Color.Lerp(colors[from], colors[to], elapsedTime / 1f);
            starsMat.SetColor(NoiseColor, r);
            //set ui colors
            var u = Color.Lerp(uiColors[from], uiColors[to], elapsedTime / 1f);
            SetUIColors(u);
            yield return null;
        }
        routine = null;
    }

    private IEnumerator SetLoop(int to, bool change)
    {
        var elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            loopPlayer.volume = 1 - elapsedTime * 2;
            yield return null;
        }

        if (!change)
        {
            loopPlayer.clip = songloops[to];
            loopPlayer.Play();
            change = true;
        }

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            loopPlayer.volume = (elapsedTime - 0.5f) * 2;
            yield return null;
        }
        routine = null;
    }

    private IEnumerator UIFadeOut(int from)
    {
        var elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(1f, 0f, elapsedTime / 0.1f);
            var b = Mathf.Lerp(0.3f, 0f, elapsedTime / 0.1f);
            rightButton.color =
                leftButton.color =
                songInfoFrame.color =
                playBtn.color =
                new Color(uiColors[from].r, uiColors[from].g, uiColors[from].b, a);
            songBkg.color = new Color(uiColors[from].r, uiColors[from].g, uiColors[from].b, b);
            songTitle.color = artistName.color = new Color(uiColors[from].r, uiColors[from].g, uiColors[from].b, a);
            yield return null;
        }
        fadeRoutine = StartCoroutine(UIFadeIn(cur));
    }

    private IEnumerator UIFadeIn(int to)
    {
        ToTransparent();
        yield return new WaitForEndOfFrame();
        var elapsedTime = 0f;
        while (elapsedTime < 0.15f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.15f);
            var b = Mathf.Lerp(0f, 0.3f, elapsedTime / 0.15f);
            rightButton.color =
                leftButton.color =
                songInfoFrame.color =
                playBtn.color =
                new Color(uiColors[to].r, uiColors[to].g, uiColors[to].b, a);
            songBkg.color = new Color(uiColors[to].r, uiColors[to].g, uiColors[to].b, b);
            songTitle.color = artistName.color = new Color(uiColors[to].r, uiColors[to].g, uiColors[to].b, a);
            yield return null;
        }
        fadeRoutine = null;
    }

    private void ToTransparent()
    {
        rightButton.color =
            leftButton.color =
            songInfoFrame.color =
            playBtn.color =
            new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, 0);
        songBkg.color = new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, 0);
        songTitle.color = artistName.color = new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, 0);
        songTitle.text = tracks[cur];
        artistName.text = "by " + artists[cur];
        if (lastLevel > cur) playBtn.sprite = playIcon;
        else playBtn.sprite = lockedIcon;
        foreach (var text in texts)
        {
            text.color = uiColors[cur];
        }
    }

    private void SetUIColors(Color col)
    {
        var length = uiElements.Length;
        for (var i = 0; i < length; i++)
        {
            uiElements[i].color = col;
        }
        uiElements[length - 1].color = new Color(col.r, col.g, col.b, 0.4f);
    }

    private IEnumerator FadeRoutine()
    {
        var elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
            var c = new Color(0, 0, 0, a);
            fadeLayer.color = c;
            yield return null;
        }
    }

    private IEnumerator LevelSelectRoutine()
    {
        var elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.5f);
            var c = new Color(0, 0, 0, a);
            fadeLayer.color = c;
            yield return null;
        }
        SceneManager.LoadScene(scenes[cur]);
    }
}