const { Client } = require('ssh2');

const conn = new Client();
conn.on('ready', () => {
  conn.exec("cd /app/tucolmadord && docker compose logs notification-service --tail 50", (err, stream) => {
    if (err) throw err;
    stream.on('close', (code, signal) => {
      conn.end();
    }).on('data', (data) => {
      console.log(data.toString());
    }).stderr.on('data', (data) => {
      console.error('STDERR: ' + data);
    });
  });
}).connect({
  host: '177.7.48.169',
  port: 22,
  username: 'root',
  password: 'ArroZZ12hju.,',
  readyTimeout: 30000
});
