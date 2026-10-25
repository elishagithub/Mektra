using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor {
public static class FigmaGuideReview {
 public static void Run(){
  var file="Assets/Resources/figma-four-test.json";var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(file));int aligned=0,links=0;
  foreach(var course in pack.levels){var level=course.level;var routes=new List<AuthoredRoute>();
   foreach(var link in level.connections){
    var source=level.platforms[link.from];var target=level.platforms[link.to];var orbit=new Orbit(source.outline);double best=double.MaxValue;Point launch=default,landing=default;double chosen=0;
    for(int sample=0;sample<720;sample++){
     double d=sample*orbit.length/720;orbit.Pose(d,out var a,out var normal);double hit=double.MaxValue;
     for(int edge=0;edge<target.outline.Length;edge++){double t=Geometry.Ray(a,normal,target.outline[edge],target.outline[(edge+1)%target.outline.Length]);if(t>0&&t<hit)hit=t;}
     if(hit==double.MaxValue)continue;
     var b=a+normal*hit;double cost=hit;
     foreach(var star in level.stars){var middle=new Point(star.x,star.y);double t=((middle-a).x*normal.x+(middle-a).y*normal.y)/hit;if(t>.12&&t<.88)cost+=Geometry.Segment(middle,a,b).distance*.3;}
     if(cost<best){best=cost;launch=a;landing=b;chosen=d;}
    }
    if(best==double.MaxValue)throw new Exception("No straight guide in "+level.id+" from "+link.from+" to "+link.to);
    routes.Add(new AuthoredRoute{from=link.from,to=link.to,launch=launch,landing=landing,distance=chosen});links++;
   }
   foreach(var star in level.stars){double best=double.MaxValue;AuthoredRoute selected=null;double fraction=.5;
    foreach(var route in routes){var delta=route.landing-route.launch;double t=((star.x-route.launch.x)*delta.x+(star.y-route.launch.y)*delta.y)/Math.Max(1,delta.x*delta.x+delta.y*delta.y);t=Math.Clamp(t,.15,.85);var at=route.launch+delta*t;double miss=(at-new Point(star.x,star.y)).Length;if(miss<best){best=miss;selected=route;fraction=t;}}
    if(selected==null)throw new Exception("No guide for "+star.id);var p=selected.launch+(selected.landing-selected.launch)*fraction;
    star.x=p.x;star.y=p.y;star.guideFrom=selected.from;star.guideTo=selected.to;star.guideFraction=fraction;star.parentPlatformId="";aligned++;
   }
   course.routes=routes.ToArray();level.authoredRadius=GameConfig.Radius;
  }
  File.WriteAllText(file,JsonUtility.ToJson(pack));Debug.Log("MEKTRA_GUIDE_ALIGNMENT_PASS links="+links+" points="+aligned);
 }
}}
