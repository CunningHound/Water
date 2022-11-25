using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WaveProperties
{
    public WaveProperties(float _direction, float _speed, float _wavelength, float _amplitude)
    {
        direction = _direction;
        speed = _speed;
        wavelength = _wavelength;
        amplitude = _amplitude;
    }

    public float direction { get; set; }
    public float speed { get; set; }
    public float wavelength { get; set; }
    public float amplitude { get; set; }

}

public class WaveManager : MonoBehaviour
{
    public Material waterMaterial;

    public int windDirection;
    public int windSpeed;

    List<WaveProperties> waves;
    [Range(0,4)]
    public int maxActiveWaves;

    // Start is called before the first frame update
    void Start()
    {
        waves = new List<WaveProperties>();
        for (int i = 0; i < maxActiveWaves; i++)
        {
            WaveProperties wave = new WaveProperties(windDirection + Random.Range(-10f*(i+1), 10f*(i+1)), (i+1)*(i+1), 10f*windSpeed/((3*i)+1), windSpeed/((i+1)*(i+1)));
            waves.Add(wave);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector4 directions = new Vector4(0f,0f,0f,0f);
        //Vector4 wavelengths = new Vector4(0f,0f,0f,0f);
        //Vector4 amplitudes = new Vector4();

        //for(int i = 0; i < maxActiveWaves; i++)
        //{
        //    directions[i] = waves[i].direction;
        //    wavelengths[i] = waves[i].wavelength;
        //    amplitudes[i] = waves[i].amplitude;
        //}

        //waterMaterial.SetVector("_Direction", directions);
        //waterMaterial.SetVector("_Wavelength", wavelengths);
        //waterMaterial.SetVector("_Amplitude", amplitudes);
    }
}
