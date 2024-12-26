using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(physicexplo), true)]
[CanEditMultipleObjects]
public class physicexplo_Edittor : Editor
{
	public string label;
	public override void OnInspectorGUI()
	{
		physicexplo myScript = (physicexplo)target;
		DrawDefaultInspector();
		GUILayout.Space(5);
		
		if (GUILayout.Button("Khởi tạo Conponent"))
		{
			MeshRenderer[] meshRenderers= myScript.GetComponentsInChildren<MeshRenderer>();
			myScript.key = new BoxCollider[meshRenderers.Length];
			myScript.physic = new Rigidbody[meshRenderers.Length];
			myScript.physicstrans = new Transform[meshRenderers.Length];
			myScript.previousLocation = new Vector3[meshRenderers.Length];
			myScript.previousRotation = new Quaternion[meshRenderers.Length];
			if (meshRenderers!=null)
            {
				for (int i = 0; i < meshRenderers.Length; i++)
				{
					BoxCollider boxCollider= meshRenderers[i].gameObject.GetComponent<BoxCollider>();
                    if (boxCollider==null)
                    {
						myScript.key[i] = meshRenderers[i].gameObject.AddComponent<BoxCollider>();
					}
                    else
                    {
						myScript.key[i] = boxCollider;
					}

					Rigidbody rigidbody  = meshRenderers[i].gameObject.GetComponent<Rigidbody>();

					if (rigidbody == null)
					{
						rigidbody = meshRenderers[i].gameObject.AddComponent<Rigidbody>();
					}
                    if (rigidbody!=null)
                    {
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
						rigidbody.useGravity = false;
						myScript.physic[i] = rigidbody;

					}
					meshRenderers[i].gameObject.layer = 25;
					myScript.physicstrans[i] = meshRenderers[i].transform;
					myScript.previousLocation[i] = meshRenderers[i].transform.localPosition;
					myScript.previousRotation[i] = meshRenderers[i].transform.localRotation;
					if (myScript.centerOfExplosion == null)
					{
						GameObject gameObject = new GameObject(name = "CenterOfExplosion");
						gameObject.transform.SetParent(myScript.transform);
						gameObject.transform.position = myScript.transform.position+new Vector3(Random.Range(-0.5f,0.5f), Random.Range(-3.5f, -5f), Random.Range(-0.5f, 0.5f));
						myScript.centerOfExplosion = gameObject.transform;
					}
                    if (myScript.centerOfExplosion==null)
                    {
						myScript.centerOfExplosion = myScript.transform;

					}

				}
			}
		}
	}
}
