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
        float master, musicMaster, blend, duck = 1, duckHold;
        bool focused = true;
        bool musicNeedsStart = true;
        float musicCheckAfter;
        Phase phase = Phase.Ready;
        public bool Enabled { get; private set; }
        public bool MusicEnabled { get; private set; }
        public float MusicVolume=>MusicEnabled?PlayerPrefs.GetFloat("mektra.music.volume",.65f):0;
        public float SoundVolume=>Enabled?PlayerPrefs.GetFloat("mektra.sound.volume",.85f):0;
        public void SetMusicVolume(float value){PlayerPrefs.SetFloat("mektra.music.volume",Mathf.Clamp01(value));SetMusic(value>0);}
        public void SetSoundVolume(float value){PlayerPrefs.SetFloat("mektra.sound.volume",Mathf.Clamp01(value));SetSound(value>0);}
        void Awake()
        {
            Enabled = PlayerPrefs.GetInt("mektra.sound", 1) == 1;
            MusicEnabled = PlayerPrefs.GetInt("mektra.music", Enabled?1:0) == 1;
            voices = new AudioSource[8];
            for (int i = 0; i < voices.Length; i++) voices[i] = Source();
            foreach (string name in new[] { "jump", "star", "land", "hello", "crash" })
                clips[name] = Resources.Load<AudioClip>("Audio/mektra_" + name);
            clips["magnet"] = clips["hello"];
            var notes=new float[28800];for(int sample=0;sample<notes.Length;sample++){double t=sample/48000.0,value=0;for(int n=0;n<3;n++){double local=t-n*.065;if(local<0)continue;double hz=n==0?659.25:n==1?830.61:987.77;value+=System.Math.Sin(local*hz*System.Math.PI*2)*System.Math.Min(1,local/.009)*System.Math.Exp(-local*9)*.22;}notes[sample]=(float)value;}var perfectClip=AudioClip.Create("Perfect landing chime",notes.Length,1,48000,false);perfectClip.SetData(notes,0);clips["perfect"]=perfectClip;
            var harmony=new float[33600];for(int i=0;i<harmony.Length;i++){double t=i/48000.0,value=0;foreach(double hz in new[]{523.25,659.25,783.99})value+=System.Math.Sin(t*hz*System.Math.PI*2)*System.Math.Min(1,t/.012)*System.Math.Exp(-t*7)*.16;harmony[i]=(float)value;}
            var milestone=AudioClip.Create("Perfect streak milestone",harmony.Length,1,48000,false);milestone.SetData(harmony,0);clips["perfect-milestone"]=milestone;
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
            if (!MusicEnabled || !focused || menuMusic == null || gameMusic == null || Time.realtimeSinceStartup < musicCheckAfter) return;
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
            master = Mathf.MoveTowards(master, focused ? 1 : 0, dt / .25f);
            musicMaster = Mathf.MoveTowards(musicMaster, MusicEnabled && focused ? 1 : 0, dt / .25f);
            blend = Mathf.MoveTowards(blend, phase == Phase.Playing ? 1 : 0, dt / 1.5f);
            duckHold = Mathf.Max(0, duckHold - dt);
            duck = Mathf.MoveTowards(duck, duckHold > 0 ? .32f : 1, dt / (duckHold > 0 ? .08f : .9f));
            float pauseLevel = phase == Phase.Paused ? .45f : 1;
            menuMusic.volume = Mathf.Cos(blend * Mathf.PI * .5f) * .22f * musicMaster * MusicVolume * duck * pauseLevel;
            gameMusic.volume = Mathf.Sin(blend * Mathf.PI * .5f) * .25f * musicMaster * MusicVolume * duck * pauseLevel;
            foreach (var voice in voices) voice.volume = Enabled?master*SoundVolume:0;
        }
        public void Play(string name,bool chimes=false,int streak=0)
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
            if(name=="perfect"){
                int[] climb={0,2,4,5,7,9,11,12,14,16};
                voice.pitch=Mathf.Pow(2,climb[Mathf.Clamp(streak-1,0,climb.Length-1)]/12f);
                if(streak==3||streak==5||streak==10)clip=clips["perfect-milestone"];
            }
            voice.panStereo = name == "star" ? ((starVoice++ % 2 == 0) ? -.12f : .12f) : 0;
            voice.volume = master*SoundVolume;
            float gain = name == "star" ? .06f : name == "crash" ? .55f : name == "land" ? .14f : name == "jump" ? .1596f : name == "perfect" ? .65f : .38f;
            voice.PlayOneShot(clip, name=="star"&&chimes?gain*.65f:gain);
        }
        public void Toggle()=>SetSound(!Enabled);
        public void SetSound(bool enabled)
        {
            Enabled=enabled;PlayerPrefs.SetInt("mektra.sound",enabled?1:0);PlayerPrefs.Save();
            if(!enabled)Stop();
        }
        public void SetMusic(bool enabled)
        {
            MusicEnabled=enabled;PlayerPrefs.SetInt("mektra.music",enabled?1:0);PlayerPrefs.Save();
            if(enabled){musicCheckAfter=0;EnsureMusic();}
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
