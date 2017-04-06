﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    private float oldTime = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void PauseTime()
    {
        oldTime = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void UnPauseTime()
    {
        Time.timeScale = oldTime;
    }
}