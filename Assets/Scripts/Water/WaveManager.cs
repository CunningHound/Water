using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WaveProperties
{
    public WaveProperties(float _direction, float _steepness, float _wavelength)
    {
        direction = _direction;
        steepness = _steepness;
        wavelength = _wavelength;
    }

    public float direction { get; set; }
    public float steepness { get; set; }
    public float wavelength { get; set; }
}

public class WaveManager : MonoBehaviour
{
    public Material waterMaterial;

    public int windDirection;
    public int windSpeed;

    public float minWavelength;
    public float minSteepness;
    List<WaveProperties> waves;
    WaveProperties transitionWave;
    private int numWaves = 4;

    public float waveTransitionPeriod;
    public float waveTransitionDuration;
    private bool waveTransitionInProgress;
    private int decreasingWaveIndex;

    private float timeSinceTransition;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceTransition = 0;
        waves = new List<WaveProperties>();
        for (int i = 0; i < numWaves; i++)
        {
            waves.Add(GetNewWave());
        }
        transitionWave = new WaveProperties(0f, 0f, 1f);
    }

    void FixedUpdate()
    {
        if(!waveTransitionInProgress)
        {
            Debug.Log("no transition in progress");
            timeSinceTransition += Time.deltaTime;
            Debug.Log("time since = " + timeSinceTransition);
            if( timeSinceTransition > waveTransitionPeriod )
            {
                Debug.Log("Starting transition");
                StartWaveTransition();
            }
        }
        else
        {

            setShaderWaveProperties();
        }
    }

    void StartWaveTransition()
    {
        Debug.Log("starting wave transition");
        if(waveTransitionInProgress)
        {
            Debug.Log("exiting early, already in progress");
            return; // only transition one wave at a time
        }
        timeSinceTransition = 0;
        waveTransitionInProgress = true;

        StartCoroutine(TransitionWave());
    }

    IEnumerator TransitionWave()
    {
        Debug.Log("initiating transition");
        int waveToReplace = Random.Range(0, waves.Count);
        float originalSteepness = waves[waveToReplace].steepness;

        WaveProperties targetWave = GetNewWave();

        float transitionTime = 0;
        while(transitionTime < waveTransitionDuration)
        {
            Debug.Log("in transition, transition time = " + transitionTime + " of " + waveTransitionDuration);
            WaveProperties adjustedWave = waves[waveToReplace];
            adjustedWave.steepness = Mathf.Lerp(originalSteepness, 0, transitionTime/waveTransitionDuration);
            waves[waveToReplace] = adjustedWave;
            Debug.Log("decreasing steepness = " + waves[waveToReplace].steepness);

            transitionWave = targetWave;
            float transitionSteepness = Mathf.Lerp(0, targetWave.steepness, transitionTime / waveTransitionDuration);
            Debug.Log("transition steepness = " + transitionSteepness);
            transitionWave.steepness = Mathf.Lerp(0, targetWave.steepness, transitionTime / waveTransitionDuration);
            transitionTime += Time.deltaTime;
            Debug.Log("setting shader values");
            yield return null;
        }

        Debug.Log("completed transition");

        waves[waveToReplace] = targetWave;
        waveTransitionInProgress = false;
        timeSinceTransition = 0;
    }

    WaveProperties GetNewWave()
    {
        return new WaveProperties(Random.Range(windDirection - 30f, windDirection + 30f), Random.Range(minSteepness, 0.25f), Random.Range(minWavelength, windSpeed));
    }

    void setShaderWaveProperties()
    {
        waterMaterial.SetVector("_Wave1", new Vector4(waves[0].direction, waves[0].steepness, waves[0].wavelength, 0));
        waterMaterial.SetVector("_Wave2", new Vector4(waves[1].direction, waves[1].steepness, waves[1].wavelength, 1));
        waterMaterial.SetVector("_Wave3", new Vector4(waves[2].direction, waves[2].steepness, waves[2].wavelength, 2));
        waterMaterial.SetVector("_Wave4", new Vector4(waves[3].direction, waves[3].steepness, waves[3].wavelength, 3));

        // transition wave, set steepness to zero if no transition in progress
        // waterMaterial.SetVector("_Wave5", new Vector4(transitionWave.direction, waveTransitionInProgress ? transitionWave.steepness : 0f, transitionWave.wavelength));
    }

}
