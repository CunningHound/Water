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
    static public WaveManager _instance;

    static public WaveManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new WaveManager();
        }
        return _instance;
    }

    public Material waterMaterial;

    public int windDirection;
    public int windSpeed;

    public float minWavelength;
    public float minSteepness;
    List<WaveProperties> waves;
    private int numWaves = 4;
    private WaveProperties transitionWave;
    private WaveProperties targetWave;
    private int waveToReplace;

    public float waveTransitionPeriod;
    public float waveTransitionDuration;
    private bool waveTransitionInProgress;

    private float timeSinceTransition;
    private float timeInTransition;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        timeSinceTransition = 0;
        timeInTransition = 0;
        waves = new List<WaveProperties>();
        for (int i = 0; i < numWaves; i++)
        {
            waves.Add(GetNewWave());
        }
        transitionWave = new WaveProperties(0f, 0f, 1f);
    }

    void FixedUpdate()
    {
        if (!waveTransitionInProgress)
        {
            timeSinceTransition += Time.deltaTime;
            if (timeSinceTransition > waveTransitionPeriod)
            {
                StartWaveTransition();
            }
        }
        else
        {
            handleWaveTransition();
        }
        SetShaderWaveProperties();
    }

    void StartWaveTransition()
    {
        if(waveTransitionInProgress)
        {
            return; // only transition one wave at a time
        }
        timeSinceTransition = 0;
        timeInTransition = 0;
        waveTransitionInProgress = true;

        targetWave = GetNewWave();
        waveToReplace = Random.Range(0, waves.Count);

        handleWaveTransition();
    }

    void handleWaveTransition()
    {
        float originalSteepness = waves[waveToReplace].steepness;

        if (timeInTransition < waveTransitionDuration)
        {
            WaveProperties adjustedWave = waves[waveToReplace];
            adjustedWave.steepness = Mathf.Lerp(originalSteepness, 0, timeInTransition / waveTransitionDuration);
            waves[waveToReplace] = adjustedWave;

            transitionWave = targetWave;
            float transitionSteepness = Mathf.Lerp(0, targetWave.steepness, timeInTransition / waveTransitionDuration);
            transitionWave.steepness = Mathf.Lerp(0, targetWave.steepness, timeInTransition / waveTransitionDuration);
            timeInTransition += Time.deltaTime;
        }
        else
        {
            waves[waveToReplace] = targetWave;
            waveTransitionInProgress = false;
            timeSinceTransition = 0;
        }
        return;
    }

    WaveProperties GetNewWave()
    {
        return new WaveProperties(Random.Range(windDirection - 30f, windDirection + 30f), Random.Range(minSteepness,windSpeed*0.01f), Random.Range(minWavelength, windSpeed));
    }

    void SetShaderWaveProperties()
    {
        waterMaterial.SetVector("_Wave1", new Vector4(waves[0].direction, waves[0].steepness, waves[0].wavelength, 0));
        waterMaterial.SetVector("_Wave2", new Vector4(waves[1].direction, waves[1].steepness, waves[1].wavelength, 1));
        waterMaterial.SetVector("_Wave3", new Vector4(waves[2].direction, waves[2].steepness, waves[2].wavelength, 2));
        waterMaterial.SetVector("_Wave4", new Vector4(waves[3].direction, waves[3].steepness, waves[3].wavelength, 3));

        //transition wave, set steepness to zero if no transition in progress
        waterMaterial.SetVector("_Wave5", new Vector4(transitionWave.direction, waveTransitionInProgress ? transitionWave.steepness : 0f, transitionWave.wavelength));
    }

    float EvaluateWaveAt(WaveProperties wave, Vector3 pos)
    {
        float k = 2 * Mathf.PI / wave.wavelength;
        float c = Mathf.Sqrt(9.8f / k);

        float x = Mathf.Cos(Mathf.Deg2Rad * wave.direction);
        float z = Mathf.Sin(Mathf.Deg2Rad * wave.direction);
        Vector2 direction = new Vector2(x,z).normalized;

        float f = k * Vector2.Dot(direction, new Vector2(pos.x, pos.z)) - c * Time.timeSinceLevelLoad;
        float a = wave.steepness / k;

        return (-a * Mathf.Cos(f));
    }

    public float GetWaterHeightAt(Vector3 pos)
    {
        float height = 0f;
        foreach (var wave in waves)
        {
            if (wave.steepness > 0f)
            {
                height += EvaluateWaveAt(wave, pos);
            }
        }

        if(waveTransitionInProgress)
        {
            height += EvaluateWaveAt(transitionWave, pos);
        }

        return height;
    }
}
