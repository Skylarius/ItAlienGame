using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.FPS.Game
{


    public class MusicManager : MonoBehaviour
    {
        public AudioSource mainMusic;
        public AudioSource secodaryMusic;

        public void ChangeTrack(AudioClip newSoundTrack)
        {
            AudioSource swap = mainMusic;
            mainMusic = secodaryMusic;
            secodaryMusic = swap;
            mainMusic.clip = newSoundTrack;
            mainMusic.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.Music1);
            secodaryMusic.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.Music2);
            StartCoroutine(FadeAudio(5f));
        }

        IEnumerator FadeAudio(float time)
        {
            mainMusic.Play();
            float t = 0.0f;
            while (mainMusic.volume < 1f)
            {
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
                float volume = Mathf.Lerp(0, 1, t / time);
                mainMusic.volume = volume;
                secodaryMusic.volume = 1f - volume;
            }
            if (secodaryMusic.volume <= 0)
                secodaryMusic.Stop();
        }
    }
}
