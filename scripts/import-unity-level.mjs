// Stage Figma edits, run real Unity reachability checks, then promote atomically on success.
import fs from 'node:fs';
import path from 'node:path';
import {execFileSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import svgpath from 'svgpath';
import {svgPathProperties} from 'svg-path-properties';
const root=fileURLToPath(new URL('../',import.meta.url));
const args=process.argv.slice(2),source=args[0],levelNumber=Number(args[1]||1);
if(!source)throw Error('Usage: node scripts/import-unity-level.mjs edited.svg LEVEL_NUMBER (or Mektra-edited-levels.json)');
const packName=args[2]||'endless';
if(!['endless','challenges','worlds','pack-20','pack-25'].includes(packName))throw Error('Unknown level pack '+packName);
const runtimeName=packName==='endless'?'endless-pack':packName==='challenges'?'challenge-pack':packName==='worlds'?'world-pack':packName.replace('pack','level-pack');
const packPath=path.join(root,'assets/levels',packName,'pack.json'),pack=JSON.parse(fs.readFileSync(packPath,'utf8'));
const edits=source.endsWith('.svg')?{levels:[{number:levelNumber,svg:fs.readFileSync(source,'utf8')}]}:JSON.parse(fs.readFileSync(source,'utf8'));
const staging=path.join(root,'assets/levels/import-staging');fs.mkdirSync(staging,{recursive:true});
const area=pts=>pts.reduce((sum,p,i)=>{const q=pts[(i+1)%pts.length];return sum+p.x*q.y-q.x*p.y},0)/2;
const updated=[];
for(const edit of edits.levels){
 const course=pack.levels.find(c=>c.number===edit.number);if(!course)throw Error('Unknown level '+edit.number);
 const file=path.join(staging,`level-${edit.number}.svg`);fs.writeFileSync(file,edit.svg);
 const extracted=JSON.parse(execFileSync('python3',[path.join(root,'scripts/read-level-svg.py'),file,'--named-only'],{encoding:'utf8'}));
 if(extracted.width!==390||extracted.height!==course.level.height)throw Error('Export the exact 390 × '+course.level.height+' level frame, without its heading.');
 const platforms=[],stars=[],hazards=[],ids=new Set();
 for(const item of extracted.items){
  const nameParts=item.id.split('__');item.id=nameParts[0];
  if(ids.has(item.id))throw Error('Duplicate or compound platform/star: '+item.id+' (flatten compound layers before export)');ids.add(item.id);
  const normalized=svgpath(item.d).matrix(item.matrix).unarc().unshort().abs();
  // Painter2D expects explicit line endpoints, including SVG H/V shorthand.
  let px=0,py=0,startX=0,startY=0;
  const commands=normalized.segments.map(seg=>{
   let [op,...v]=seg;if(op==='H'){v=[v[0],py];op='L'}if(op==='V'){v=[px,v[0]];op='L'}
   if(op==='M'){startX=v[0];startY=v[1]}if(op==='Z'){px=startX;py=startY}else{px=v.at(-2);py=v.at(-1)}
   if(!['M','L','C','Q','Z'].includes(op))throw Error('Unsupported path operation '+op);
   return {op,v};
  });
  const d=commands.map(c=>c.op+c.v.join(' ')).join(' '),contours=(d.match(/[M][^M]*/g)||[]).map(part=>{
   const property=new svgPathProperties(part),length=property.getTotalLength();
   return Array.from({length:Math.max(96,Math.ceil(length/2))},(_,i,all)=>property.getPointAtLength(i/Math.max(96,Math.ceil(length/2))*length));
  });
  const outline=contours.sort((a,b)=>Math.abs(area(b))-Math.abs(area(a)))[0];if(!outline?.length)throw Error('Empty path '+item.id);
  if(area(outline)<0)outline.reverse();
  const bounds={minX:Math.min(...outline.map(p=>p.x)),maxX:Math.max(...outline.map(p=>p.x)),minY:Math.min(...outline.map(p=>p.y)),maxY:Math.max(...outline.map(p=>p.y))};
  if(item.role==='hole'){
   const hole=(course.level.blackHoles||[]).find(h=>h.id===item.id);if(!hole)throw Error('Unknown black hole '+item.id);
   const radius=(bounds.maxX-bounds.minX)/2;hole.pullRadius*=radius/hole.radius;hole.radius=radius;hole.x=(bounds.minX+bounds.maxX)/2;hole.y=(bounds.minY+bounds.maxY)/2;
  }else if(item.role==='bounce'){
   const match=item.id.match(/^bounce-(\d+)-point-(\d+)$/);const run=match&&course.level.bounceRuns?.[Number(match[1])];
   if(!run||!run.points[Number(match[2])])throw Error('Unknown bounce marker '+item.id);
   run.points[Number(match[2])]={x:(bounds.minX+bounds.maxX)/2,y:(bounds.minY+bounds.maxY)/2};
  }else if(item.role==='platform'||item.role==='hazard'){
   const collection=item.role==='hazard'?hazards:platforms;
   const previous=item.role==='hazard'?(course.level.hazards||[]):course.level.platforms;
   const old=previous.find(p=>p.id===item.id);
   collection.push({crumbleSeconds:old?.crumbleSeconds??0,label:old?.label??'',id:item.id,sourceShapeId:old?.sourceShapeId??item.id.match(/custom-shape-\d+/)?.[0]??'',d,paths:commands,outline,bounds,motion:edit.motions?.[item.id]??((nameParts.length===6||nameParts.length===7)?{type:nameParts[1],amplitudeX:Number(nameParts[2]),amplitudeY:Number(nameParts[3]),periodSeconds:Number(nameParts[4]),phase:Number(nameParts[5]),speedVariation:nameParts.length===7?Number(nameParts[6]):old?.motion?.speedVariation??0}:old?.motion??null)});
  }else{
   const property=new svgPathProperties(d),length=property.getTotalLength(),samples=Array.from({length:500},(_,i)=>property.getPointAtLength(i/500*length));
   const old=course.level.stars.find(s=>s.id===item.id);
   stars.push({id:item.id,x:samples.reduce((s,p)=>s+p.x,0)/500,y:samples.reduce((s,p)=>s+p.y,0)/500,parentPlatformId:edit.parents?.[item.id]??(nameParts[1]==='follow'?nameParts[2]:old?.parentPlatformId??'')});
  }
 }
 if(platforms.length<2||!stars.length)throw Error('Missing named platforms or stars. Enable “Include id attribute” when exporting SVG.');
 if(pack.challengeOpening||pack.endless){const order=course.level.platforms.map(p=>p.id);if(platforms.length!==order.length||platforms.some(p=>!order.includes(p.id)))throw Error("Keep existing platform IDs when editing challenge connections.");platforms.sort((a,b)=>order.indexOf(a.id)-order.indexOf(b.id));}
 else platforms.sort((a,b)=>b.bounds.maxY-a.bounds.maxY);course.level.platforms=platforms;course.level.hazards=hazards.length?hazards:course.level.hazards||[];course.level.stars=stars;course.points=stars.length;course.level.source='Figma export: '+path.basename(source);course.routes=[];updated.push(edit.number);
}
pack.updatedLevels=updated;
const candidate=path.join(staging,'candidate-pack.json');fs.writeFileSync(candidate,JSON.stringify(pack));
const unity=process.env.MEKTRA_UNITY_EDITOR||'/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity';
try{
 execFileSync(unity,['-batchmode','-quit','-projectPath',path.join(root,'Unity'),'-executeMethod','Mektra.Editor.LevelImportValidator.Validate','-mektraCandidate',candidate,'-logFile',path.join(staging,'unity-validation.log')],{stdio:'inherit'});
}catch(error){throw Error('Import validation failed. Active levels were preserved. Read '+path.join(staging,'unity-validation.log'));}
const checked=JSON.parse(fs.readFileSync(candidate,'utf8'));delete checked.updatedLevels;
fs.copyFileSync(packPath,path.join(staging,'previous-pack.json'));
for(const [file,data] of [[packPath,JSON.stringify(checked,null,2)+'\n'],[path.join(root,'Unity/Assets/Resources',runtimeName+'.json'),JSON.stringify(checked)]]){
 fs.writeFileSync(file+'.tmp',data);fs.renameSync(file+'.tmp',file);
}
for(const n of updated){const c=checked.levels.find(c=>c.number===n);fs.writeFileSync(path.join(root,`assets/levels/${packName}/Mektra_level_${n}.json`),JSON.stringify(c,null,2)+'\n')}
console.log('Imported and validated levels '+updated.join(', ')+'. Rebuild the Mac game to play your edits.');
