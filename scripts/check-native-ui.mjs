import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const Socket = global.WebSocket ?? require('ws');
const targets = await (await fetch('http://localhost:8081/json/list')).json();
const target = targets.find(item => item.description?.includes('Bridgeless'));
assert.ok(target, 'Launch the Debug app with Metro first.');
const socket = new Socket(target.webSocketDebuggerUrl);
await new Promise((resolve, reject) => {
  socket.onopen = resolve;
  socket.onerror = reject;
});
try {
  const prefix = `MEKTRA_NATIVE_RESULT_${Date.now()}:`;
  const result = await new Promise((resolve, reject) => {
    const timer = setTimeout(
      () => reject(new Error('Native UI check timed out')),
      15000,
    );
    socket.onmessage = event => {
      const message = JSON.parse(event.data);
      if (message.method === 'Runtime.consoleAPICalled') {
        const value = message.params.args.find(
          arg => typeof arg.value === 'string' && arg.value.startsWith(prefix),
        )?.value;
        if (value) {
          clearTimeout(timer);
          resolve(JSON.parse(value.slice(prefix.length)));
        }
      }
      if (
        message.id === 1 &&
        (message.error || message.result?.exceptionDetails)
      ) {
        clearTimeout(timer);
        reject(new Error(JSON.stringify(message)));
      }
    };
    socket.send(JSON.stringify({ id: 0, method: 'Runtime.enable' }));
    socket.send(
      JSON.stringify({
        id: 1,
        method: 'Runtime.evaluate',
        params: {
          expression: `globalThis.__mektraNativeChecks().then(result => console.log(${JSON.stringify(
            prefix,
          )} + JSON.stringify(result)))`,
          awaitPromise: true,
          returnByValue: true,
        },
      }),
    );
  });
  console.log(JSON.stringify(result, null, 2));
  assert.deepEqual(result.failures, [], 'Native jump/pause regression');
  assert.ok(
    result.uiFramesDuringJSStall >= 8,
    'UI frames stopped during a 300ms JS stall',
  );
} finally {
  socket.close();
}
