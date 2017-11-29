using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {
    private float time;
    private float startTime;
    private bool repeat;
    private bool started;

    public Timer(float time, bool repeat)
    {
        this.startTime = time;
        this.time = time;
        this.repeat = repeat;
        start();
    }
    public void start()
    {
        started = true;
    }
    public void stop()
    {
        started = false;
    }
    public void update()
    {
        if(started) this.time -= 1f * Time.deltaTime;
    }
    public float getTime()
    {
        return time;
    }
    public bool hasRecentlyStarted()
    {
        if(this.time >= this.startTime) {
            return true;
        }
        return false;
    }
    public bool isFinished()
    {
        if(this.time <= 0) {
            if(repeat) this.time = this.startTime; else stop();
            return true;
        }
        return false;
    }
}
