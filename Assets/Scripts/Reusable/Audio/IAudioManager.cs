public interface IAudioManager
{
    void PlaySFX(string sfxId);
    void PlayMusic(string musicId);
    void StopMusic();
    void SetMusicVolume(float volume);
    void SetSFXVolume(float volume);
}
