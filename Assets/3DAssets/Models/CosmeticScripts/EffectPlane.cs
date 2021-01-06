using UnityEngine;

public class EffectPlane : MonoBehaviour
{
    public GameObject myPlane;
    private float rotVal;
    private Renderer myRenderer;
    private MaterialPropertyBlock block;
    private float changeVal0 = 0;
    private float changeVal1 = 0;
    private float changeVal2 = 0;
    [Range(0.0f, 1f)]
    public float Offset1;
    [Range(0.0f, 1f)]
    public float Offset2;



    private void Start()
    {
        myRenderer = myPlane.GetComponent<Renderer>();
        block = new MaterialPropertyBlock();

        myRenderer.GetPropertyBlock(block);


    }


    void Update()
    {
        if(Conductor.pauseTimeStamp > 0) return;
        //Rotation..
        rotVal = (Time.deltaTime * 10) % 360;
        myPlane.transform.Rotate(new Vector3(0, rotVal, 0));

        //Material Value Change..
        changeVal0 = Mathf.SmoothStep(0f, 0.2f , Mathf.PingPong(Time.time * 0.1f, 1f));
        changeVal1 = Mathf.SmoothStep(0f, 0.2f, Mathf.PingPong(Time.time * 0.1f + Offset1, 1f));
        changeVal2 = Mathf.SmoothStep(0f, 0.2f, Mathf.PingPong(Time.time * 0.1f + Offset2, 1f));

        block.SetFloat("_BloomLight1", changeVal0);
        block.SetFloat("_BloomLight2", changeVal1);
        block.SetFloat("_BloomLight3", changeVal2);

        myRenderer.SetPropertyBlock(block);
    }


}
