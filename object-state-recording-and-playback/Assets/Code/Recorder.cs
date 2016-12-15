using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {
    public int record_frequency = 20;

    public float record_start_time;
    public float record_length;
    public float play_start_time;

    public bool reverse = false;

    protected float last_recorded_at = -1.0f;

    protected RecordData record_data;

    protected bool recording = false;
    protected bool playing = false;

    protected Transform[] transforms;

    public void StartRecording()
    {
        transforms = GetComponentsInChildren<Transform>();

        record_data = new RecordData();
        record_data.Initialize(transforms.Length);

        last_recorded_at = -1.0f;

        record_start_time = Time.time;

        recording = true;
    }

    public void StopRecording()
    {
        recording = false;
        record_length = Time.time - record_start_time;
    }

    public void StartPlaying()
    {
        play_start_time = Time.time;
        playing = true;
    }

    public void StopPlay()
    {
        playing = false;
    }

    public void SetTo(float normalized_time)
    {
        
    }

    protected void Record()
    {
        if (last_recorded_at >= 0.0f && last_recorded_at > (Time.time - 1.0f/record_frequency))
        {
            return;
        }
        // Gather states
        RecordData.RecordObjectState[] states = new RecordData.RecordObjectState[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            RecordData.RecordObjectState state = new RecordData.RecordObjectState();
            state.position = transforms[i].localPosition;
            state.rotation = transforms[i].localRotation;
            state.scale = transforms[i].localScale;
            state.active = transforms[i].gameObject.activeSelf;
            state.transform = transforms[i];

            states[i] = state;
        }

        record_data.SaveStates(states, Time.time - record_start_time);

        last_recorded_at = Time.time;
    }

    protected void Play()
    {
        float time = Time.time - play_start_time;

        if (reverse)
        {
            time = record_length - time;
            if (time < 0.0f)
                time = 0.0f;
        }

        RecordData.RecordObjectState[] states = record_data.GetInterpolatedStates(time);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].localPosition = states[i].position;
            transforms[i].localRotation = states[i].rotation;
            //transforms[i]scale thing
        }

        
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!recording)
                StartRecording();
            else
                StopRecording();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!playing)
                StartPlaying();
            else
                StopPlay();
        }

        if (recording)
            Record();
        if (playing)
            Play();
    }
}
