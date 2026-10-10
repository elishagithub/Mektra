import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = fileURLToPath(new URL('../', import.meta.url));
function run(command, args, capture = false) {
  const result = spawnSync(command, args, {
    cwd: root,
    encoding: 'utf8',
    stdio: capture ? 'pipe' : 'inherit',
  });
  if (result.error) throw result.error;
  if (result.status !== 0) {
    if (capture) process.stderr.write(result.stderr || result.stdout || '');
    process.exit(result.status || 1);
  }
  return result.stdout;
}

// CLI 20 assumes Simulator.app exists. Xcode 27 uses Device Hub instead;
// simctl builds and launches without depending on either desktop app.
const devices = Object.values(
  JSON.parse(
    run('xcrun', ['simctl', 'list', 'devices', 'available', '--json'], true),
  ).devices,
)
  .flat()
  .filter(device => device.name.startsWith('iPhone'));
const configuration = process.argv.includes('--release') ? 'Release' : 'Debug';
const requested = process.argv.slice(2).find(value => !value.startsWith('--'));
const device = requested
  ? devices.find(item => item.name === requested || item.udid === requested)
  : devices.find(item => item.state === 'Booted') || devices[0];
if (!device)
  throw new Error(
    'No matching iPhone simulator. Install an iOS runtime in Xcode.',
  );
if (device.state !== 'Booted') run('xcrun', ['simctl', 'boot', device.udid]);
run('xcrun', ['simctl', 'bootstatus', device.udid, '-b']);
const derivedData = path.join(root, 'ios/build-simulator');
run('xcodebuild', [
  '-workspace',
  'ios/Mektra.xcworkspace',
  '-scheme',
  'Mektra',
  '-configuration',
  configuration,
  '-sdk',
  'iphonesimulator',
  '-destination',
  `platform=iOS Simulator,id=${device.udid}`,
  '-derivedDataPath',
  derivedData,
  '-jobs',
  '8',
  'CODE_SIGNING_ALLOWED=NO',
]);
const app = path.join(
  derivedData,
  `Build/Products/${configuration}-iphonesimulator/Mektra.app`,
);
run('xcrun', ['simctl', 'install', device.udid, app]);
const bundleId = run(
  '/usr/libexec/PlistBuddy',
  ['-c', 'Print CFBundleIdentifier', path.join(app, 'Info.plist')],
  true,
).trim();
run('xcrun', ['simctl', 'launch', device.udid, bundleId]);
console.log(
  `Mektra is running on ${device.name}. View it in Xcode Device Hub (or Simulator on older Xcode).`,
);
