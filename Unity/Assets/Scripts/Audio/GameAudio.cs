using System.Collections.Generic;
using UnityEngine;
namespace Mektra
{
    // Music shares one DSP start and tempo, so crossfades never produce competing beats.
    public class GameAudio : MonoBehaviour
    {
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, double> lastPlayed = new Dictionary<string, double>();
        AudioSource[] voices;
        AudioSource menuMusic, gameMusic;
        int cursor, starVoice, starCursor;
        static readonly int[] ChimeNotes={0,2,4,7,9,7,4,2};
        float master, blend, duck = 1, duckHold;
        bool focused = true;
        bool musicNeedsStart = true;
        float musicCheckAfter;
        Phase phase = Phase.Ready;
        public bool Enabled { get; private set; }
        void Awake()
        {
            Enabled = PlayerPrefs.GetInt("mektra.sound", 1) == 1;
            voices = new AudioSource[8];
            for (int i = 0; i < voices.Length; i++) voices[i] = Source();
            foreach (string name in new[] { "jump", "star", "land", "hello", "crash" })
                clips[name] = Resources.Load<AudioClip>("Audio/mektra_" + name);
            clips["magnet"] = clips["hello"];
            menuMusic = Music("music_menu"); gameMusic = Music("music_game");
            EnsureMusic();
        }
        AudioSource Source()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0;
            source.dopplerLevel = 0; source.volume = 0;
            return source;
        }
        AudioSource Music(string name)
        {
            var source = Source(); source.clip = Resources.Load<AudioClip>("Audio/mektra_" + name);
            source.loop = true; source.priority = 32; return source;
        }
        void OnEnable() { AudioSettings.OnAudioConfigurationChanged += AudioConfigurationChanged; }
        void OnDisable() { AudioSettings.OnAudioConfigurationChanged -= AudioConfigurationChanged; }
        void AudioConfigurationChanged(bool deviceChanged)
        {
            // Device resets can discard scheduled sources even though their clips remain loaded.
            musicNeedsStart = true;
            musicCheckAfter = Time.realtimeSinceStartup + .25f;
        }
        void EnsureMusic()
        {
            if (!Enabled || !focused || menuMusic == null || gameMusic == null || Time.realtimeSinceStartup < musicCheckAfter) return;
            if (!musicNeedsStart && menuMusic.isPlaying && gameMusic.isPlaying) return;
            // Restart together only after a lost source/reset, preserving the original matched loops.
            menuMusic.Stop(); gameMusic.Stop();
            double start = AudioSettings.dspTime + .15;
            if (menuMusic.clip != null) menuMusic.PlayScheduled(start);
            if (gameMusic.clip != null) gameMusic.PlayScheduled(start);
            musicNeedsStart = false;
            // Do not treat a source waiting for its scheduled DSP start as a stopped loop.
            musicCheckAfter = Time.realtimeSinceStartup + 1;
            Debug.Log("MEKTRA_MUSIC_STARTED: menu and gameplay loops scheduled together.");
        }
        public void SetPhase(Phase value) { phase = value; }
        void Update()
        {
            EnsureMusic();
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            master = Mathf.MoveTowards(master, Enabled && focused ? 1 : 0, dt / .25f);
            blend = Mathf.MoveTowards(blend, phase == Phase.Playing ? 1 : 0, dt / 1.5f);
            duckHold = Mathf.Max(0, duckHold - dt);
            duck = Mathf.MoveTowards(duck, duckHold > 0 ? .32f : 1, dt / (duckHold > 0 ? .08f : .9f));
            float pauseLevel = phase == Phase.Paused ? .45f : 1;
            menuMusic.volume = Mathf.Cos(blend * Mathf.PI * .5f) * .22f * master * duck * pauseLevel;
            gameMusic.volume = Mathf.Sin(blend * Mathf.PI * .5f) * .25f * master * duck * pauseLevel;
            foreach (var voice in voices) voice.volume = master;
        }
        public void Play(string name,bool chimes=false)
        {
            if (!Enabled || !focused || !clips.TryGetValue(name, out var clip) || clip == null) return;
            double now = AudioSettings.dspTime;
            double gap = name == "star" ? .045 : name == "land" ? .12 : .06;
            if (lastPlayed.TryGetValue(name, out double last) && now - last < gap) return;
            lastPlayed[name] = now;
            if (name == "crash") { Stop(); duckHold = .85f; }
            // Reuse an idle voice first; stop a stolen voice to bound simultaneous peaks.
            AudioSource voice = null;
            if (name == "star")
            {
                voice = voices[starCursor++ % 3]; voice.Stop();
            }
            else
            {
                for (int i = 3; i < voices.Length; i++) if (!voices[i].isPlaying) { voice = voices[i]; break; }
                if (voice == null) { voice = voices[3 + cursor++ % (voices.Length - 3)]; voice.Stop(); }
            }
            voice.pitch = name=="star"&&chimes?Mathf.Pow(2,ChimeNotes[(starCursor-1)%ChimeNotes.Length]/12f):1;
            voice.panStereo = name == "star" ? ((starVoice++ % 2 == 0) ? -.12f : .12f) : 0;
            voice.volume = master;
            float gain = name == "star" ? .26f : name == "crash" ? .55f : name == "land" ? .14f : name == "jump" ? .1596f : .38f;
            voice.PlayOneShot(clip, name=="star"&&chimes?gain*.65f:gain);
        }
        public void Toggle()
        {
            Enabled = !Enabled;
            PlayerPrefs.SetInt("mektra.sound", Enabled ? 1 : 0); PlayerPrefs.Save();
            if (Enabled) { musicCheckAfter = 0; EnsureMusic(); }
        }
        // Pause clears transient effects; music fades independently without restarting its loop.
        public void Stop() { if (voices != null) foreach (var voice in voices) voice.Stop(); }
        void OnApplicationFocus(bool value)
        {
            focused = value;
            if (!value)
            {
                Stop(); master = 0;
                if (menuMusic != null) menuMusic.Pause();
                if (gameMusic != null) gameMusic.Pause();
            }
            else
            {
                if (menuMusic != null) menuMusic.UnPause();
                if (gameMusic != null) gameMusic.UnPause();
                musicCheckAfter = Time.realtimeSinceStartup + .25f;
            }
        }
        void OnApplicationPause(bool value) { OnApplicationFocus(!value); }
    }
}
