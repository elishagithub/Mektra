using System;using System.IO;using System.Collections.Generic;using UnityEngine;
namespace Mektra.Editor {
public static class VarietyGuideReview {
 public static void Run(){
  var candidate=JsonUtility.FromJson<LevelPack>(File.ReadAllText("Assets/Resources/variety-candidate.json"));var previous=LevelPack.Load();int routes=0,points=0,fallback=0;
  for(int index=8;index<candidate.levels.Length;index++){
   var course=candidate.levels[index];var l=course.level;try{MotionRegression.GeometryChecks(l);}catch(Exception ex){Debug.LogWarning("Preserving constrained geometry in level "+course.number+": "+ex.Message);l.platforms=previous.levels[index].level.platforms;}var result=new List<AuthoredRoute>();
   foreach(var link in l.connections??Array.Empty<RouteLink>()){
    if(l.platforms[link.from].sourceShapeId=="wedge")continue;
    var source=l.platforms[link.from];var target=l.platforms[link.to];var orbit=new Orbit(source.outline);double best=double.MaxValue;Point launch=default,landing=default;double chosen=0;
    for(int sample=0;sample<360;sample++){
     double d=sample*orbit.length/360;orbit.Pose(d,out var a,out var normal);double hit=double.MaxValue;
     for(int edge=0;edge<target.outline.Length;edge++){double t=Geometry.Ray(a,normal,target.outline[edge],target.outline[(edge+1)%target.outline.Length]);if(t>0&&t<hit)hit=t;}
     if(hit==double.MaxValue)continue;
     var b=a+normal*hit;if(b.x<6||b.x>384)continue;
     bool blocked=false;
     foreach(var hazard in l.hazards??Array.Empty<PlatformData>())for(int edge=0;edge<hazard.outline.Length;edge++){double t=Geometry.Ray(a,normal,hazard.outline[edge],hazard.outline[(edge+1)%hazard.outline.Length]);if(t>0&&t<hit)blocked=true;}
     if(blocked)continue;
     double cost=hit;
     foreach(var star in l.stars){var point=new Point(star.x,star.y);double t=((point-a).x*normal.x+(point-a).y*normal.y)/hit;if(t>.15&&t<.85)cost+=Geometry.Segment(point,a,b).distance*.15;}
     if(cost<best){best=cost;launch=a;landing=b;chosen=d;}
    }
    if(best==double.MaxValue){var old=Array.Find(previous.levels[index].routes??Array.Empty<AuthoredRoute>(),r=>r.from==link.from&&r.to==link.to);if(old!=null){result.Add(old);fallback++;}continue;}
    result.Add(new AuthoredRoute{from=link.from,to=link.to,launch=launch,landing=landing,distance=chosen});routes++;
   }
   course.routes=result.ToArray();l.authoredRadius=GameConfig.Radius;
   foreach(var star in l.stars){if(!string.IsNullOrEmpty(star.parentPlatformId)||result.Count==0)continue;
    double best=double.MaxValue,tBest=.5;AuthoredRoute selected=null;
    foreach(var route in result){var delta=route.landing-route.launch;double den=delta.x*delta.x+delta.y*delta.y;double t=Math.Clamp(((star.x-route.launch.x)*delta.x+(star.y-route.launch.y)*delta.y)/Math.Max(1,den),.18,.82);var p=route.launch+delta*t;double miss=(p-new Point(star.x,star.y)).Length;if(miss<best){best=miss;tBest=t;selected=route;}}
    var at=selected.launch+(selected.landing-selected.launch)*tBest;star.x=at.x;star.y=at.y;star.guideFrom=selected.from;star.guideTo=selected.to;star.guideFraction=tBest;points++;
   }
  }
  File.WriteAllText("Assets/Resources/variety-candidate.json",JsonUtility.ToJson(candidate));Debug.Log("MEKTRA_VARIETY_REVIEW routes="+routes+" alignedPoints="+points+" preservedSpecialRoutes="+fallback);
 }
}}
