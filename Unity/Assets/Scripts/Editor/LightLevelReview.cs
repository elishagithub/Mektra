using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor {
public static class LightLevelReview {
 public static void Run(){
  var pack=LevelPack.Load();var chars=CharacterCatalog.Load();int reviewed=0,removed=0,aligned=0;
  foreach(var c in pack.levels){var l=c.level;var routes=new List<AuthoredRoute>();var links=new List<RouteLink>();
   bool simple=l.segmentKind==0||l.segmentKind==1||l.segmentKind==5||l.segmentKind==12||l.segmentKind==15||l.segmentKind==16;
   if(!simple)continue;
   foreach(var route in c.routes){
    CourseReview.JumpReview r;
    try{r=CourseReview.Review(l,route.from,chars,route.to);}catch(Exception){Debug.LogWarning("LIGHT_ROUTE_UNREACHABLE "+l.id+" "+route.from+" -> "+route.to);removed++;continue;}
    routes.Add(new AuthoredRoute{from=r.from,to=r.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});links.Add(new RouteLink{from=r.from,to=r.to});reviewed++;
   }
   if(routes.Count==0)throw new Exception("No playable routes: "+l.id);
   c.routes=routes.ToArray();l.connections=links.ToArray();
   foreach(var star in l.stars){if(!string.IsNullOrEmpty(star.parentPlatformId))continue;double best=double.MaxValue;Point chosen=new Point(star.x,star.y);
    foreach(var route in routes){var a=route.launch;var delta=route.landing-a;double den=delta.x*delta.x+delta.y*delta.y;double t=((star.x-a.x)*delta.x+(star.y-a.y)*delta.y)/Math.Max(1,den);if(t<.12||t>.88)continue;var at=a+delta*t;double d=(at-new Point(star.x,star.y)).Length;if(d<best){best=d;chosen=at;}}
    if(best<70){star.x=chosen.x;star.y=chosen.y;aligned++;}
   }
  }
  File.WriteAllText("Assets/Resources/endless-pack.json",JsonUtility.ToJson(pack));
  Debug.Log("MEKTRA_LIGHT_LEVEL_REVIEW_PASS routes="+reviewed+" unreachable="+removed+" aligned="+aligned);
 }
}}
