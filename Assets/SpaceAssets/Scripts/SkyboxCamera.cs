using UnityEngine;
using System.Collections;

public class SkyboxCamera : MonoBehaviour 
{
	public Camera mainCamera;
	public Camera skyboxCamera;
	public GameObject spaceSkybox;

    void OnEnable()
    {
        spaceSkybox.transform.position = transform.position;
        spaceSkybox.transform.rotation = Quaternion.identity;
        spaceSkybox.transform.parent = transform;
        spaceSkybox.gameObject.layer = gameObject.layer;
    }
    
	void OnPreCull()
    {
        spaceSkybox.transform.rotation = Quaternion.identity;
    }
	
	void LateUpdate()
    {
		skyboxCamera.transform.rotation = mainCamera.transform.rotation;
    }
}
