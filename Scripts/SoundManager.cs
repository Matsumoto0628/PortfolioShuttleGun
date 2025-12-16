using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;
    Dictionary<string, AudioClip> clipDictionary;
    Dictionary<string, AudioSource> sourceDictionary;
    [SerializeField] private AudioClip jumpStart;
    [SerializeField] private AudioClip jumpEnd;
    [SerializeField] private AudioClip assultFire;
    [SerializeField] private AudioClip assultHit;
    [SerializeField] private AudioClip reloadStart;
    [SerializeField] private AudioClip reloadEnd;
    [SerializeField] private AudioClip sliding;
    [SerializeField] private AudioClip climbing;
    [SerializeField] private AudioClip mantle;
    [SerializeField] private AudioClip footSteps;
    [SerializeField] private AudioClip limit;
    [SerializeField] private AudioClip glass;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioSource basicSource;
    [SerializeField] private AudioSource pitchSource;
    [SerializeField] private AudioSource sourcePrefab;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }

        instance = this;

        clipDictionary = new Dictionary<string, AudioClip>();
        sourceDictionary = new Dictionary<string, AudioSource>();
        clipDictionary.Add("jumpStart", jumpStart);
        clipDictionary.Add("jumpEnd", jumpEnd);
        clipDictionary.Add("assultFire", assultFire);
        clipDictionary.Add("assultHit", assultHit);
        clipDictionary.Add("reloadStart", reloadStart);
        clipDictionary.Add("reloadEnd", reloadEnd);
        clipDictionary.Add("sliding", sliding);
        clipDictionary.Add("climbing", climbing);
        clipDictionary.Add("mantle", mantle);
        clipDictionary.Add("footSteps", footSteps);
        clipDictionary.Add("limit", limit);
        clipDictionary.Add("glass", glass);
        clipDictionary.Add("explosion", explosion);
    }

    public void PlaySE(string clipName)
    {
        basicSource.pitch = 1f;
        basicSource.PlayOneShot(clipDictionary[clipName]);
    }

    public void PlaySE(string name, float randomPercent)
    {
        basicSource.pitch = 1f + Random.Range(-randomPercent / 100, randomPercent / 100);
        basicSource.PlayOneShot(clipDictionary[name]);
    }
    
    public void PlayPitchSE(string clipName, float pitch)
    {
        pitchSource.pitch = pitch;
        pitchSource.PlayOneShot(clipDictionary[clipName]);
    }

    public void PlayLoopSE(string clipName)
    {
        if (sourceDictionary.ContainsKey(clipName))
        {
            if (!sourceDictionary[clipName].isPlaying)
                sourceDictionary[clipName].Play();
        }
        else
        {
            AudioSource audioSource = Instantiate(sourcePrefab, transform);
            audioSource.clip = clipDictionary[clipName];
            sourceDictionary.Add(clipName, audioSource);
            audioSource.loop = true;
        }
    }

    public void StopLoopSE(string clipName)
    {
        if (sourceDictionary.ContainsKey(clipName))
            sourceDictionary[clipName].Stop();
    }
}