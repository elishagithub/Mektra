using System;using System.IO;using UnityEngine;
namespace Mektra.Editor {
 public static class LiquidSurfaceReview {
  public static void Run(){int checkedRoutes=0,reverted=0;
   foreach(var name in new[]{"endless-pack","figma-four-test"}){
    var file="Assets/Resources/"+name+".json";var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(file));var backup=JsonUtility.FromJson<LevelPack>(File.ReadAllText("../assets/levels/design-review/"+name+"-before-liquid.json"));var chars=CharacterCatalog.Load();
    for(int k=0;k<pack.levels.Length;k++){
     var l=pack.levels[k].level;
     foreach(var link in l.connections??Array.Empty<RouteLink>()){
      bool liquid=l.platforms[link.from].sourceShapeId.StartsWith("liquid-smooth-")||l.platforms[link.to].sourceShapeId.StartsWith("liquid-smooth-");if(!liquid)continue;
      try{CourseReview.Review(l,link.from,chars,link.to);checkedRoutes++;}
      catch(Exception e){Debug.LogWarning("LIQUID_PRESERVE_ROUTE level="+pack.levels[k].number+" "+e.Message);l.platforms[link.from]=backup.levels[k].level.platforms[link.from];l.platforms[link.to]=backup.levels[k].level.platforms[link.to];reverted++;}
     }
    }
    File.WriteAllText(file,JsonUtility.ToJson(pack));
   }
   Debug.Log("MEKTRA_LIQUID_PLAYABILITY_PASS routes="+checkedRoutes+" preservedLinks="+reverted);
   FigmaGuideReview.Run();MektraBuild.BuildInvertPreview();
  }
 }
}
