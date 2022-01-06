using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public AudioSource audioSource;
    public Lane[] lanes;
    public float songDelayInSeconds;
    public double marginOfError; // in seconds

    public int inputDelayInMilliseconds;

    public static MidiFile midiFile;

    public bool itStarted = false;

    private string fileLocation;
    public float noteTime;
    public float noteSpawnY;
    public float noteTapY;
    internal float noteDespawnY       
    {
        get
        {
            return noteTapY - (noteSpawnY - noteTapY);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        fileLocation = "let_it_go_" + PlayerPrefs.GetString("SelectedGesture") + ".mid";


        Instance = this;
        
        ReadFromFile();
    }

    void Update()
    {

    }

    private void ReadFromFile()
    {
        midiFile = MidiFile.Read(Application.dataPath + "/" + fileLocation);
        GetDataFromMidi();
    }
    public void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes();
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0);

        foreach (var lane in lanes) lane.SetTimeStamps(array);

        Invoke(nameof(StartSong), songDelayInSeconds);
        
    }
    public void StartSong()
    {
        audioSource.Play();
        itStarted = true;
    }
    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

   
}