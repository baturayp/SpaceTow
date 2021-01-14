using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR)
public class StagePalette : MonoBehaviour
{

    public GameObject generalMatObjs;
    public GameObject planet;
    public GameObject mLayer;
    public InfiniteStarField myStarField;
    private Renderer[] rendererIndex;
    public Material[] materialIndex;
    public GameObject[] skyCanvas;
    public Image planetLight;
    public enum colorState
    {
        OrangeCyan,
        Synthwave1,
        Synthwave2,
        GreenYellow,
        BlueOrange
    }

    public colorState curState;


    public class ColorSettings
    {
        public Color mainColor;
        public Color shadowColor;
        public Color lightColor;
        public Color rimColor;
        public Color starColor1;
        public Color starColor2;
        public Color planetLightColor;
        public Color meteor;
    }

    public ColorSettings OrangeCyan;
    public ColorSettings GreenYellow;
    public ColorSettings Synthwave1;
    public ColorSettings Synthwave2;
    public ColorSettings BlueOrange;

    public MaterialPropertyBlock block;

    [ExecuteInEditMode]

    void Start()
    {
        rendererIndex = new Renderer[3];
        OrangeCyan = new ColorSettings();
        Synthwave1 = new ColorSettings();
        Synthwave2 = new ColorSettings();
        GreenYellow = new ColorSettings();
        BlueOrange = new ColorSettings();

        rendererIndex[0] = generalMatObjs.GetComponent<Renderer>();
        rendererIndex[1] = planet.GetComponent<Renderer>();
        rendererIndex[2] = mLayer.GetComponent<Renderer>();

        OrangeCyan.mainColor = new Color(.82f, .71f, .62f, 1f);
        OrangeCyan.shadowColor = new Color(.034f, .29f, .24f, 1f);
        OrangeCyan.lightColor = new Color(1f, .60f, .16f, 1f);
        OrangeCyan.rimColor = new Color(.66f, .40f, .15f, 1f);
        OrangeCyan.starColor1 = new Color(1f, .9f, .46f, 1f);
        OrangeCyan.starColor2 = new Color(.42f, 1f, .93f, 1f);
        OrangeCyan.planetLightColor = new Color(.93f,.90f,.69f,1f);
        OrangeCyan.meteor = new Color(1f,1f,1f,1f);

        Synthwave1.mainColor = new Color(.7f, .7f, .7f, 1f);
        Synthwave1.shadowColor = new Color(.43f, .06f, .52f, 1f);
        Synthwave1.lightColor = new Color(.45f, .16f, 1f, 1f);
        Synthwave1.rimColor = new Color(.75f, .43f, .14f, 1f);
        Synthwave1.starColor1 = new Color(.95f, .46f, 1f, 1f);
        Synthwave1.starColor2 = new Color(1f, 42f, .51f, 1f);
        Synthwave1.planetLightColor = new Color(.75f, .37f, .45f, 1f);
        Synthwave1.meteor = new Color(1f, 1f, 1f, 1f);

        Synthwave2.mainColor = new Color(.63f, .36f, .70f, 1f);
        Synthwave2.shadowColor = new Color(.12f, .06f, .52f, 1f);
        Synthwave2.lightColor = new Color(.54f, .09f, .83f, 1f);
        Synthwave2.rimColor = new Color(.23f, .54f, .59f, 1f);
        Synthwave2.starColor1 = new Color(0f, .94f, 1f, 1f);
        Synthwave2.starColor2 = new Color(.26f, 0f, 1f, 1f);
        Synthwave2.planetLightColor = new Color(.24f, .56f, .83f, 1f);
        Synthwave2.meteor = new Color(1f, 1f, 1f, 1f);


        GreenYellow.mainColor = new Color(.73f, .72f, .66f, 1f);
        GreenYellow.shadowColor = new Color(.33f, .32f, .18f, 1f);
        GreenYellow.lightColor = new Color(.17f, 1f, .09f, 1f);
        GreenYellow.rimColor = new Color(.56f, .58f, .24f, 1f);
        GreenYellow.starColor1 = new Color(.35f, 1f, .33f, 1f);
        GreenYellow.starColor2 = new Color(.97f, 1f, .5f, 1f);
        GreenYellow.planetLightColor = new Color(.55f, .81f, .50f, 1f);
        GreenYellow.meteor = new Color(1f, 1f, 1f, 1f);

        BlueOrange.mainColor = new Color(.85f, .80f, .54f, 1f);
        BlueOrange.shadowColor = new Color(.70f, .31f, .04f, 1f);
        BlueOrange.lightColor = new Color(.17f, .09f, .83f, 1f);
        BlueOrange.rimColor = new Color(.16f, .58f, .58f, 1f);
        BlueOrange.starColor1 = new Color(0f, .94f, 1f, 1f);
        BlueOrange.starColor2 = new Color(1f, .33f, 0f, 1f);
        BlueOrange.meteor = new Color(1f, 1f, 1f, 1f);


        block = new MaterialPropertyBlock();

        if (curState == colorState.OrangeCyan)
        {
            for (int i = 0; i < materialIndex.Length; i++)
            {
                if (i == 0)
                    materialIndex[i].SetColor("_MainColor", OrangeCyan.mainColor);
                else
                    materialIndex[i].SetColor("_MainColor", Color.white);

                materialIndex[i].SetColor("_ShadowColor", OrangeCyan.shadowColor);
                materialIndex[i].SetColor("_LightC", OrangeCyan.lightColor);
                materialIndex[i].SetColor("_RimLight", OrangeCyan.rimColor);
            }
            Instantiate(skyCanvas[0]);
            planetLight.color = OrangeCyan.planetLightColor;
            myStarField.starColor1 = OrangeCyan.starColor1;
            myStarField.starColor2 = OrangeCyan.starColor2;
        }
        if (curState == colorState.Synthwave1)
        {
            for (int i = 0; i < materialIndex.Length; i++)
            {
                if (i == 0)
                    materialIndex[i].SetColor("_MainColor", OrangeCyan.mainColor);
                else
                    materialIndex[i].SetColor("_MainColor", Color.white);
                materialIndex[i].SetColor("_ShadowColor", Synthwave1.shadowColor);
                materialIndex[i].SetColor("_LightC", Synthwave1.lightColor);
                materialIndex[i].SetColor("_RimLight", Synthwave1.rimColor);
            }
            Instantiate(skyCanvas[1]);
            planetLight.color = Synthwave1.planetLightColor;
            myStarField.starColor1 = Synthwave1.starColor1;
            myStarField.starColor2 = Synthwave1.starColor2;
        }
        if (curState == colorState.Synthwave2)
        {
            for (int i = 0; i < materialIndex.Length; i++)
            {
                if (i == 0)
                    materialIndex[i].SetColor("_MainColor", Synthwave2.mainColor);
                else
                    materialIndex[i].SetColor("_MainColor", Color.white);

                materialIndex[i].SetColor("_ShadowColor", Synthwave2.shadowColor);
                materialIndex[i].SetColor("_LightC", Synthwave2.lightColor);
                materialIndex[i].SetColor("_RimLight", Synthwave2.rimColor);
            }
            Instantiate(skyCanvas[2]);
            planetLight.color = Synthwave2.planetLightColor;
            myStarField.starColor1 = Synthwave2.starColor1;
            myStarField.starColor2 = Synthwave2.starColor2;
        }
        if (curState == colorState.BlueOrange)
        {
            for (int i = 0; i < materialIndex.Length; i++)
            {
                if (i == 0)
                    materialIndex[i].SetColor("_MainColor", BlueOrange.mainColor);
                else
                    materialIndex[i].SetColor("_MainColor", Color.white);
                materialIndex[i].SetColor("_ShadowColor", BlueOrange.shadowColor);
                materialIndex[i].SetColor("_LightC", BlueOrange.lightColor);
                materialIndex[i].SetColor("_RimLight", BlueOrange.rimColor);
            }
            Instantiate(skyCanvas[4]);
            planetLight.color = BlueOrange.planetLightColor;
            myStarField.starColor1 = BlueOrange.starColor1;
            myStarField.starColor2 = BlueOrange.starColor2;
        }
    }
}

#endif