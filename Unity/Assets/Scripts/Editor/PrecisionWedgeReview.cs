using System;using System.IO;using UnityEngine;
namespace Mektra.Editor {
 public static class PrecisionWedgeReview {
  public static void Run(){int passed=0,keptOut=0;var chars=CharacterCatalog.Load();
   foreach(var name in new[]{"endless-pack","figma-four-test"}){
    string file="Assets/Resources/"+name+".json";var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(file));
    foreach(var c in pack.levels){var l=c.level;var hazards=l.hazards??Array.Empty<PlatformData>();var links=new System.Collections.Generic.HashSet<int>();
     foreach(var h in hazards)if(h.sourceShapeId=="precision-lethal-wedge"){var parts=h.id.Split('-');links.Add(int.Parse(parts[2]));}
     bool safe=true;foreach(int from in links){var link=Array.Find(l.connections,x=>x.from==from);if(link==null)continue;try{CourseReview.Review(l,from,chars,link.to);passed++;}catch(Exception e){safe=false;Debug.LogWarning("WEDGE_PRESERVE_OPEN_ROUTE "+c.number+": "+e.Message);}}
     if(!safe){l.hazards=Array.FindAll(hazards,h=>h.sourceShapeId!="precision-lethal-wedge");keptOut+=links.Count;}
    }
    File.WriteAllText(file,JsonUtility.ToJson(pack));
   }
   Debug.Log("MEKTRA_PRECISION_WEDGES_PASS routes="+passed+" rejected="+keptOut);MektraBuild.BuildInvertPreview();
  }
 }
}
