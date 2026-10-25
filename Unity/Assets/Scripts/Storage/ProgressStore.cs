using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra
{
    [Serializable] public class Progress
    {
        public int bank,best,accuracyBest;
        public string selected="mektra";
        public string nickname="";
        public string leaderboardPlayerId="";
        public List<string> unlocked=new List<string>
        {
            "mektra"
        }
        ;
    }
    public class ProgressStore
    {
        public Progress Data
        {
            get;
            private set;
        }
        public int Best{
get{
#if MEKTRA_INVERT_PREVIEW
return Data.accuracyBest;
#else
return Data.best;
#endif
}}
        public string Path=>System.IO.Path.Combine(Application.persistentDataPath,"mektra.progress.v1.json");
        public ProgressStore()
        {
            try
            {
                Data=File.Exists(Path)?JsonUtility.FromJson<Progress>(File.ReadAllText(Path)):new Progress();
            }
            catch(Exception e)
            {
                Debug.LogWarning("Could not load saved progress: "+e.Message);
                Data=new Progress();
            }
            if(Data==null)Data=new Progress();
            Data.bank=Math.Max(0,Data.bank);
            Data.best=Math.Max(0,Data.best);
            if(Data.unlocked==null)Data.unlocked=new List<string>();
            if(!Data.unlocked.Contains("mektra"))Data.unlocked.Add("mektra");
            if(!Data.unlocked.Contains(Data.selected))Data.selected="mektra";
        }
        public bool Choose(Character c)
        {
            if(!Data.unlocked.Contains(c.id))
            {
                if(Data.bank<c.cost)return false;
                Data.bank-=c.cost;
                Data.unlocked.Add(c.id);
            }
            Data.selected=c.id;
            Save();
            return true;
        }
        public void Finish(int stars,int distance)
        {
            Data.bank+=stars;
            #if MEKTRA_INVERT_PREVIEW
            Data.accuracyBest=Math.Max(Data.accuracyBest,distance);
#else
            Data.best=Math.Max(Data.best,distance);
#endif
            Save();
        }
        public void Save()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string temp=Path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(Data));
            if(File.Exists(Path))File.Delete(Path);
            File.Move(temp,Path);
        }
    }
}
