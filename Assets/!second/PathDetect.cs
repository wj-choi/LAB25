﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDetect : MonoBehaviour
{
	public PathFinder pathFinder;
	public FadeTrigger fadeTrigger;
	public Her0inAgent her0inAgent;

	bool checkOnce;

	void Start()
	{
		her0inAgent = GetComponent<Her0inAgent>();
		this.gameObject.transform.position = pathFinder.destinations[pathFinder.destinationsIndex] + Vector3.up * 1f;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("agent"))
		{
			if (pathFinder.destinationsIndex < pathFinder.maxIndex - 1)
			{
				pathFinder.destinationsIndex++;
				this.gameObject.transform.position = pathFinder.destinations[pathFinder.destinationsIndex] + Vector3.up * 1f;
			}

			else
			{
				// this.gameObject.SetActive(false);
				other.gameObject.GetComponent<Her0inAgent>().agentRunSpeed = 0;
				fadeTrigger.fadeInOut();
			}
		}
	}
}