using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exhausts : MonoBehaviour
{
    [SerializeField] AudioSource TankAudioSource, ExhaustAudioSource;
    [SerializeField] AudioClip Engine_Idle, Engine_Accelerate;

    #region Particles
    ParticleSystem smokeParticles, bubbleParticles, bubbleParticlesSM;
    ParticleSystem.MainModule smokeMain, bubbleMain, bubbleSMMain;
    ParticleSystem.EmissionModule smokeEmission, bubbleEmission, bubbleSMEmission;
    #endregion

    #region SoundEffects
    float idleEngineMinPitch = 1f, idleEngineMaxPitch = 1.6f;

    float accEngineMinPitch = 0.7f, accEngineMaxPitch = 1.3f;
    float accEngineMinVolume = 0f, accEngineMaxVolume = 0.1f;
    #endregion

    PlayerController PlayerCTRL;
    bool isTankAnimReset;


    private void Start()
    {
        TankAudioSource.clip = Engine_Accelerate;
        TankAudioSource.volume = 0;

        PlayerCTRL = transform.parent.parent.GetComponent<PlayerController>();

        #region SetParticles
        smokeParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
        bubbleParticles = transform.GetChild(1).GetComponent<ParticleSystem>();
        bubbleParticlesSM = bubbleParticles.transform.GetChild(0).GetComponent<ParticleSystem>();

        bubbleParticles.Stop();
        bubbleParticlesSM.Stop();

        smokeMain = smokeParticles.main;
        smokeEmission = smokeParticles.emission;

        bubbleMain = bubbleParticles.main;
        bubbleSMMain = bubbleParticlesSM.main;
        bubbleEmission = bubbleParticles.emission;
        bubbleSMEmission = bubbleParticlesSM.emission;
        #endregion
    }

    private void Update()
    {
        if (PlayerCTRL.isInTank && !PlayerCTRL.isInBunker)
        {
            PlayTankAnim();
            isTankAnimReset = false;
        }
        else if (!isTankAnimReset)
        {
            ResetTankAnim();
            isTankAnimReset = CheckAnimReset();
        }

        if (!PlayerCTRL.engineOn)
        {
            TurnEngineOff();
        }
    }

    void PlayTankAnim()
    {
        //Altera valores de som e partículas de fumaça de acordo com a aceleração
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            TankAudioSource.volume = Mathf.Clamp(TankAudioSource.volume + Time.deltaTime / 6, accEngineMinVolume, accEngineMaxVolume);
            TankAudioSource.pitch = Mathf.Clamp(TankAudioSource.pitch + Time.deltaTime, accEngineMinPitch, accEngineMaxPitch);
            ExhaustAudioSource.pitch = Mathf.Clamp(TankAudioSource.pitch + Time.deltaTime, idleEngineMinPitch, idleEngineMaxPitch);

            smokeMain.startLifetime = Mathf.Clamp(smokeMain.startLifetime.constant + Time.deltaTime, 0.7f, 1);
            smokeMain.startSpeed = Mathf.Clamp(smokeMain.startSpeed.constant + Time.deltaTime, 0.7f, 1);
            smokeMain.startSize = 0.3f;
            smokeMain.startColor = Color.gray;
            smokeEmission.rateOverTime = 15;

            bubbleMain.startSpeed = Mathf.Clamp(bubbleMain.startSpeed.constant + Time.deltaTime, 0.5f, 1);
            bubbleSMMain.startSpeed = Mathf.Clamp(bubbleMain.startSpeed.constant + Time.deltaTime, 0.5f, 1);
            bubbleEmission.rateOverTime = 5;
            bubbleSMEmission.rateOverTime = 15;
        }
        else
        {
            ResetTankAnim();
        }
    }

    void ResetTankAnim()
    {
        TankAudioSource.volume = Mathf.Clamp(TankAudioSource.volume - Time.deltaTime / 6, accEngineMinVolume, accEngineMaxVolume);
        TankAudioSource.pitch = Mathf.Clamp(TankAudioSource.pitch - Time.deltaTime, accEngineMinPitch, accEngineMaxPitch);
        ExhaustAudioSource.pitch = Mathf.Clamp(TankAudioSource.pitch - Time.deltaTime, idleEngineMinPitch, idleEngineMaxPitch);

        smokeMain.startLifetime = Mathf.Clamp(smokeMain.startLifetime.constant - Time.deltaTime, 0.7f, 1);
        smokeMain.startSpeed = Mathf.Clamp(smokeMain.startSpeed.constant - Time.deltaTime, 0.7f, 1);
        smokeMain.startSize = 0.2f;
        smokeMain.startColor = new Color(255, 255, 255);
        smokeEmission.rateOverTime = 7;

        bubbleMain.startSpeed = Mathf.Clamp(bubbleMain.startSpeed.constant - Time.deltaTime, 0.5f, 1);
        bubbleSMMain.startSpeed = Mathf.Clamp(bubbleMain.startSpeed.constant - Time.deltaTime, 0.5f, 1);
        bubbleEmission.rateOverTime = 2;
        bubbleSMEmission.rateOverTime = 8;
    }

    void TurnEngineOff()
    {
        TankAudioSource.volume = 0;
        ExhaustAudioSource.volume = 0;
        smokeParticles.Stop();
        bubbleParticles.Stop();
    }

    bool CheckAnimReset()
    {
        return
            TankAudioSource.volume == accEngineMinVolume &&
            TankAudioSource.pitch == accEngineMinPitch &&
            ExhaustAudioSource.pitch == idleEngineMinPitch;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Tilemap_0")
        {
            smokeParticles.Stop();
            bubbleParticles.Play();
            bubbleParticlesSM.Play();

            idleEngineMinPitch = 0.8f;
            idleEngineMaxPitch = 1.3f;
            accEngineMinPitch = 0.5f;
            accEngineMaxPitch = 1.1f;
            accEngineMaxVolume = 0.07f;
            ExhaustAudioSource.volume = 0.07f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Tilemap_0")
        {
            smokeParticles.Play();
            bubbleParticles.Stop();
            bubbleParticlesSM.Stop();

            idleEngineMinPitch = 1f;
            idleEngineMaxPitch = 1.6f;
            accEngineMinPitch = 0.7f;
            accEngineMaxPitch = 1.3f;
            accEngineMaxVolume = 0.1f;
            ExhaustAudioSource.volume = 0.125f;
        }
    }
}
