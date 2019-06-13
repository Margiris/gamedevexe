const WebSocket = require('ws');
const exec = require("child_process").exec;
let portScanner = require("portscanner");

const NETWORK_MANAGER_PORT = 7777;
const SERVER_PORT = 8000;
// const PORT_OFFSET = 10000;
const MAX_PORT = 20000;

// create new websocket server
const wss = new WebSocket.Server({port: SERVER_PORT});

// on new client connect
wss.on('connection', function connection(client) {
    console.log('new client connected');
    // on new message received
    client.on('message', function incoming(data) {
        // get data from string
        let [a, b] = data.toString().split('\t');
        let requestingServerStart = (a === 'True');
        let port = parseInt(b);

        let result = 3;
        /* result values:
        0 - port not occupied and server not running on that port
        1 - port in use by BioDude server
        2 - port in use by another process
         */

        console.log(requestingServerStart, port);

        let cmd = 'powershell (Get-Process -Id (Get-NetTCPConnection -LocalPort ' + port + ').OwningProcess).Name';
        // stdout is a string containing the output of the command.
        exec(cmd, function (err, stdout, stderr) {
            let suggestedPort;

            // if port is currently not occupied by any process or is in use by BioDude server
            if ((stderr !== '' || stdout.indexOf('BioDude') !== -1) && port !== NETWORK_MANAGER_PORT) {
                result = stdout.indexOf('BioDude') !== -1 ? 1 : 0;
                suggestedPort = port;
                // send response
                client.send(JSON.stringify({result, suggestedPort}));
            } else {
                result = 2;
                getRandomFreePortInRange(port + 1, MAX_PORT, function (port) {
                    suggestedPort = port;
                    // send response
                    client.send(JSON.stringify({result, suggestedPort}));
                });
            }

            if (result === 0 && requestingServerStart) {
                cmd = '"M:\\KTU\\3 k. 2 s\\Interaktyvios interneto technologijos\\BioDude\\BioDude.exe" ' + port;
                console.log(cmd);
                exec(cmd, function (err, stdout, stderr) {
                });
            }
        });


    })
});

function getRandomFreePortInRange(portFrom, portTo, callback) {
    portScanner.findAPortNotInUse(portFrom, portTo, function (error, port) {
        callback(port);
    });
}
