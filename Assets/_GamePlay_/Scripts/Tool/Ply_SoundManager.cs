using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FxType
{
    Pop = 0,
    CanMerge = 1,
    MergeDone = 2,
}

public class Ply_SoundManager : Ply_Singleton<Ply_SoundManager>
{
    public AudioClip[] audioClips;
    public AudioSource sound;
    private AudioSource[] fx = new AudioSource[5];

    bool isMute = false;

    public void PlayFx(FxType fxType)
    {
        if (!isMute)
        {
            if (fx[(int)fxType] == null)
            {
                fx[(int)fxType] = new GameObject("Audio_" + fxType).AddComponent<AudioSource>();
                fx[(int)fxType].clip = audioClips[(int)fxType];
            }
            fx[(int)fxType].PlayOneShot(audioClips[(int)fxType]);
        }
    }

    public void Mute()
    {
        sound.Stop();
        for (int i = 0; i < fx.Length; i++)
        {
            if (fx[i] != null)
            {
                fx[i].Stop();
            }
        }
    }
}
