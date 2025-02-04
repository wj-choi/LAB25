﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
	Projector projector;
	Zemmer zemmer;

	[Header("Default Settings")]
	public float speed = 5f;
	public float viewRadius;
	public float circleSize = 7f;

	[Header("Zemmer")]
	public bool isUseable;
	public LayerMask playerMask;
	public bool isExploding;

	void Awake()
	{
		projector = GetComponent<Projector>();
		if(zemmer == null) zemmer = FindObjectOfType<Zemmer>();
		StartCoroutine(Init());
	}

	IEnumerator Init()
	{
		while (projector.orthographicSize < circleSize)
		{
			projector.orthographicSize = Mathf.Clamp(projector.orthographicSize += speed * Time.deltaTime, 0f, circleSize);
			yield return null;
		}
		if (isUseable) StartCoroutine(ZemmerUpdate());
		yield return null;
	}

	IEnumerator ZemmerUpdate()
	{
		while (true)
		{
			if (zemmer == null) break;

			if (!zemmer.activate)
			{
				projector.orthographicSize = Mathf.Clamp(projector.orthographicSize += speed * Time.deltaTime, 3f, circleSize);
			}

			else
			{
				if (!isExploding)
				{
					Collider[] playerInRadius = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

					if (playerInRadius.Length != 0)
					{
						if (projector.orthographicSize > 3f)
							projector.orthographicSize -= speed * Time.deltaTime;
					}

					else if (playerInRadius.Length == 0)
					{
						if (projector.orthographicSize < circleSize)
							projector.orthographicSize += speed * Time.deltaTime;
					}

					projector.orthographicSize = Mathf.Clamp(projector.orthographicSize, 3f, circleSize);
				}

				else
				{
					if (projector.orthographicSize < circleSize)
						projector.orthographicSize += speed * Time.deltaTime;
					projector.orthographicSize = Mathf.Clamp(projector.orthographicSize, 3f, circleSize);
				}
			}

			yield return null;
		}
	}

	public IEnumerator DecreaseCircle()
	{
		while (projector.orthographicSize >= 0f)
		{
			projector.orthographicSize = Mathf.Clamp(projector.orthographicSize -= speed * Time.deltaTime, 0f, circleSize);
			yield return null;
		}
	}

	//private void OnDrawGizmosSelected()
	//{
	//	Gizmos.color = Color.red;
	//	Gizmos.DrawWireSphere(this.transform.position, viewRadius);
	//}
}
