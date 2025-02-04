﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PostProcessing;

public class GameIntroScene : MonoBehaviour
{
	[TextArea(3, 10)]
	public string[] mainTitleSentences;
	[TextArea(3, 10)]
	public string[] subTitleSentences;

	public Vector2 RandomThreshold;

	int sentenceIndex;

	public TextMeshProUGUI mainTitleText;
	public Text subTitleText;


	public Animator fadeAnimator;
	public MoveToNextScene moveToNextScene;

	public PostProcessingProfile ppProfile;
	public float blinkStartSecond;
	bool isBlinkEnd;

	void Start()
	{
		mainTitleText.text = mainTitleSentences[sentenceIndex];
		subTitleText.text = subTitleSentences[sentenceIndex];

		StartCoroutine(Blink());
        StartCoroutine(CheckSkipScene());
    }

    private IEnumerator CheckSkipScene()
    {
		while (true)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				isBlinkEnd = true;
				fadeAnimator.SetTrigger("SceneEnd");
				Invoke("LoadScene", 1.5f);
				break;
			}
			yield return null;
		}
    }

	public void LoadScene()
	{
		//Debug.Log("Skip!");
		moveToNextScene.LoadSceneTrigger();
	}

	void SetPostprocessing()
	{
		BloomModel.Settings bloomSettings = ppProfile.bloom.settings;
		bloomSettings.bloom.threshold = Random.Range(RandomThreshold.x, RandomThreshold.y);
		ppProfile.bloom.settings = bloomSettings;
	}

	IEnumerator Blink()
	{
		while(!isBlinkEnd)
		{
			yield return new WaitForSeconds(blinkStartSecond);
			SetPostprocessing();
		}

		StopAllCoroutines();
		yield return null;
	}

	public void IncreaseSentenceIndex()
	{
		sentenceIndex = Mathf.Clamp(sentenceIndex += 1, 0, mainTitleSentences.Length);
		mainTitleText.text = mainTitleSentences[sentenceIndex];
		subTitleText.text = subTitleSentences[sentenceIndex];
	}

	public CameraShake.Properties testProperties;

	public void CameraShake()
	{
		FindObjectOfType<CameraShake>().StartShake(testProperties);
	}
}
