using UnityEngine;

public class InfiniteStarField : MonoBehaviour
{

    private Transform tx;

    private ParticleSystem.Particle[] points;

    private ParticleSystem myParticleSystem;

    private float starDistanceSqr;
    private float starClipDistanceSqr;
    private Vector3 randomVal;

    public int starsMax = 100;
    public int emmissiveStars = 10;
    public float starSize = 1.0f;
    public float starDistance = 10;
    public float starClipDistance = 2;


    // Start is called before the first frame update
    void Start()
    {
        tx = transform;
        myParticleSystem = this.GetComponent<ParticleSystem>();
        starDistanceSqr = starDistance * starDistance;
        starClipDistanceSqr = starClipDistance * starClipDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (points == null)
            CreateStars();

        for (int i = 0; i < starsMax; i++)
        {
            if((points[i].position - tx.position).sqrMagnitude > starDistanceSqr || (points[i].position - tx.position).z < 0)
            {
                randomVal = Random.insideUnitSphere;

                if (randomVal.z < 0)
                    randomVal.z *= -1;
                if (randomVal.y > 0)
                    randomVal.y *= -1;

                points[i].position = randomVal * starDistance + tx.position;
            }

            if ((points[i].position - tx.position).sqrMagnitude < starDistanceSqr)
            {
                float percent = (points[i].position - tx.position).sqrMagnitude / starDistanceSqr;

                Color temp = points[i].startColor;
                points[i].startColor = new Color(temp.r, temp.g, temp.b, percent);
                points[i].startSize = percent * starSize;
            }
        }


        myParticleSystem.SetParticles(points, points.Length);
    }

    private void CreateStars()
    {
        points = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            randomVal = Random.insideUnitSphere;

            if (randomVal.z < 0)
                randomVal.z *= -1;
            if (randomVal.y > 0)
                randomVal.y *= -1;


            float col1 = Random.Range(0.8f, 1f);
            float col2 = Random.Range(0.8f, 1f);
            float col3 = Random.Range(0.8f, 1f);

            points[i].position = randomVal * starDistance + tx.position;
            points[i].startColor = new Color(col1, col2, col3, 1);
            points[i].startSize = starSize;



        }
    }
}
