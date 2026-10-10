from pathlib import Path
import re,json
from concurrent.futures import ThreadPoolExecutor
roots=['Unity/Assets','Unity/Packages','Unity/ProjectSettings','assets','figma','scripts','docs','release/store-assets','release/privacy-site','src','__tests__']
files=[]
for root in roots:
 files.extend(p for p in Path(root).rglob('*') if p.is_file() and p.name!='.DS_Store' and not any(x in p.parts for x in ['node_modules','.git','__pycache__']))
files += [Path(p) for p in ['.gitignore','README.md','README-react-native-prototype.md','release/README.md','release/store-listing.txt','App.tsx','app.json','package.json','package-lock.json','tsconfig.json','babel.config.js','metro.config.js','index.js'] if Path(p).is_file()]
blocked=[]
patterns=[re.compile(rb'gh[pousr]_[A-Za-z0-9]{25,}'),re.compile(rb'github_pat_[A-Za-z0-9_]{40,}'),re.compile(rb'AKIA[A-Z0-9]{16}'),re.compile(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----'),re.compile(rb'(?i)(?:api[_-]?key|secret[_-]?key|access[_-]?token|password)\s*[:=]\s*[\"\x27][A-Za-z0-9_+/=-]{20,}[\"\x27]')]
def inspect(p):
 if p.suffix.lower() in {'.p12','.jks','.key','.pem','.keystore','.apk','.aab','.dmg','.pkg','.zip'} or p.name.startswith('.env'):return str(p)
 data=p.read_bytes()
 if any(rx.search(data) for rx in patterns):return str(p)
 return None
with ThreadPoolExecutor(max_workers=32) as pool:
 blocked=[p for p in pool.map(inspect,files) if p]
report={'source_roots':roots,'file_count':len(files),'bytes':sum(p.stat().st_size for p in files),'blocked_files':blocked,'excluded':'Builds, Library, .utmp, node_modules, .bundle, local .env files, APK/AAB, signing folder and private-key formats'}
Path('/tmp/mektra-source-export-audit.json').write_text(json.dumps(report,indent=2))
Path('/tmp/mektra-source-export-paths').write_bytes(b'\0'.join(str(p).encode() for p in files)+b'\0')
print(json.dumps(report,indent=2))
if blocked:raise SystemExit('Source audit failed; nothing should be pushed until reviewed')
