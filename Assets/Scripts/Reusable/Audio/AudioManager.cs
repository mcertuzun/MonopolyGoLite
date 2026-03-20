using UnityEngine;

public class AudioManager : MonoBehaviour, IAudioManager
{
    float _musicVolume = 1f;
    float _sfxVolume = 1f;

    public void PlaySFX(string sfxId)
    {
        Debug.Log($"[Audio] SFX: {sfxId}");
    }

    public void PlayMusic(string musicId)
    {
        Debug.Log($"[Audio] Music: {musicId}");
    }

    public void StopMusic()
    {
        Debug.Log("[Audio] Music stopped");
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }
}
