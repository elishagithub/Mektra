using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra
{
    [Serializable] public class CampaignCheckpoint {public string levelId;public int platform;}
    [Serializable] public class DiscoveryState
    {
        public const int Count=99;
        public int current,unlocked,completedMask,best;
        public long completedBits;
        public int saveVersion;
        public List<string> completedIds=new List<string>();
        public List<CampaignCheckpoint> checkpointRecords=new List<CampaignCheckpoint>();
        public int rainLayoutVersion;
        public int[] checkpoints=new int[CampaignIdentity.Capacity];
        public double activeSeconds;
        public int[] failures=new int[CampaignIdentity.Capacity];
        public void Normalize()
        {
            completedBits|=(long)(completedMask&0x3fffffff);
            if(completedIds==null)completedIds=new List<string>();
            for(int i=0;i<63;i++)if((completedBits&(1L<<i))!=0&&!completedIds.Contains(CampaignIdentity.Id(i)))completedIds.Add(CampaignIdentity.Id(i));
            completedIds=new List<string>(new HashSet<string>(completedIds));
            if(checkpointRecords==null)checkpointRecords=new List<CampaignCheckpoint>();
            if(checkpoints==null)checkpoints=new int[CampaignIdentity.Capacity];else Array.Resize(ref checkpoints,CampaignIdentity.Capacity);
            if(rainLayoutVersion<1)
            {
                if(checkpoints[30]==3)checkpoints[30]=16;else if(checkpoints[30]==6)checkpoints[30]=32;else if(checkpoints[30]==10)checkpoints[30]=48;
                rainLayoutVersion=1;
            }
            foreach(var record in checkpointRecords){int i=CampaignIdentity.Index(record.levelId);if(i>=0)checkpoints[i]=Math.Max(0,record.platform);}
            SyncCheckpointRecords();saveVersion=2;
            unlocked=Math.Clamp(unlocked,0,Count-1);
            activeSeconds=double.IsFinite(activeSeconds)?Math.Max(0,activeSeconds):0;
            if(failures==null)failures=new int[CampaignIdentity.Capacity];else Array.Resize(ref failures,CampaignIdentity.Capacity);
            for(int i=0;i<Count;i++)if(Completed(i))unlocked=Math.Max(unlocked,Math.Min(Count-1,i+1));
            Tick(0);current=Math.Clamp(current,0,unlocked);
            while(current<unlocked&&Completed(current))current++;
        }
        public void Tick(double seconds)
        {
            activeSeconds+=Math.Max(0,seconds);
            unlocked=Math.Max(unlocked,Math.Min(Count-1,activeSeconds<600?Math.Min(19,(int)(activeSeconds/30)):20+(int)((activeSeconds-600)/60)));
        }
        public void SetCheckpoint(int index,int platform){checkpoints[index]=Math.Max(0,platform);SyncCheckpointRecords();}
        public void SyncCheckpointRecords()
        {
            if(checkpointRecords==null)checkpointRecords=new List<CampaignCheckpoint>();
            for(int i=0;i<checkpoints.Length;i++)
            {
                string id=CampaignIdentity.Id(i);var record=checkpointRecords.Find(r=>r.levelId==id);
                if(checkpoints[i]<=0){if(record!=null)checkpointRecords.Remove(record);}
                else if(record==null)checkpointRecords.Add(new CampaignCheckpoint{levelId=id,platform=checkpoints[i]});else record.platform=checkpoints[i];
            }
        }
        public void Complete(int index)
        {
            string id=CampaignIdentity.Id(index);if(id==null)return;
            if(!completedIds.Contains(id))completedIds.Add(id);
            if(index<63)completedBits|=1L<<index;if(index<30)completedMask|=1<<index;
            SetCheckpoint(index,0);unlocked=Math.Max(unlocked,Math.Min(Count-1,index+1));
        }
        public void Fail(int index){if(++failures[index]>=3)unlocked=Math.Max(unlocked,Math.Min(Count-1,index+1));}
        public void Select(int index){current=Math.Clamp(index,0,unlocked);}
        public bool Completed(int index)=>index>=0&&index<CampaignIdentity.Capacity&&(completedIds.Contains(CampaignIdentity.Id(index))||index<63&&(completedBits&(1L<<index))!=0||index<30&&(completedMask&(1<<index))!=0);

    }
    public sealed class DiscoveryProgress
    {
        const string Key="mektra.discovery.prototype-a.v1";
        public DiscoveryState State {get;private set;}
        double savedAt;
        public DiscoveryProgress()
        {
            try{State=JsonUtility.FromJson<DiscoveryState>(PlayerPrefs.GetString(Key,"{}"));}catch{State=null;}
            if(State==null)State=new DiscoveryState();
            State.Normalize();
        }
        public void Tick(double seconds){State.Tick(seconds);if(State.activeSeconds-savedAt>=5)Save();}
        public void Save(){State.SyncCheckpointRecords();PlayerPrefs.SetString(Key,JsonUtility.ToJson(State));PlayerPrefs.Save();savedAt=State.activeSeconds;}
    }
}
