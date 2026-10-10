import {execFileSync} from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const root=fileURLToPath(new URL('../',import.meta.url));
const editor=process.env.MEKTRA_UNITY_EDITOR||'/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity';
const log=path.join(root,'Unity','build-validation.log');
execFileSync(editor,['-batchmode','-quit','-projectPath',path.join(root,'Unity'),'-buildTarget','StandaloneOSX','-executeMethod','Mektra.Editor.MektraBuild.Build','-logFile',log],{stdio:'inherit'});
if(!fs.readFileSync(log,'utf8').includes('MEKTRA_BUILD_SUCCESS'))throw Error('Build did not confirm success. Read '+log);
console.log('Built Unity/Builds/Mektra.app.');
if(process.argv.includes('--launch')){
 try{execFileSync('pkill',['-x','Mektra'])}catch{} // Restart only this game; saved progress is retained.
 execFileSync('open',[path.join(root,'Unity','Builds','Mektra.app')]);
}
