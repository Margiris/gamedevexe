const WebSocket = require('ws');
import {exec} from "child_process";
import portscanner from "portscanner";

// create new websocket server
const wss = new WebSocket.Server({port: 8000});

var result = 3;

// on new client connect
wss.on('connection', function connection(client) {
    // on new message received
    client.on('message', function incoming(data) {
        // get data from string
        var [requestingServerStart, port, portOffset] = data.toString().split('\t');
        console.log(requestingServerStart, port, portOffset);

        // stdout is a string containing the output of the command.
        exec('powershell (Get-Process -Id (Get-NetTCPConnection -LocalPort ' + (port + portOffset) + ').OwningProcess).Name', function (err, stdout, stderr) {
            // if port is currently not occupied by any process or is in use by BioDude server
            if (err !== '' || stdout === 'BioDude')
                result = 0;
            else
                result = 1
        });

        if (result === 0 && requestingServerStart)
            exec('BioDude.exe ' + (port + portOffset), function (err, stdout, stderr) {
            });
    })
});

function getRandomFreePort() {
    var net = require('net');
    var server = net.createServer();

    server.once('error', function (err) {
        if (err.code === 'EADDRINUSE') {
            // port is currently in use
        }
    });

    server.once('listening', function () {
        // close the server if listening doesn't fail
        server.close();
    });

    server.listen(/* put the port to check here */);
}

function broadcastUpdate() {
    // broadcast messages to all clients
    wss.clients.forEach(function each(client) {
        // filter disconnected clients
        if (client.readyState !== WebSocket.OPEN) return;
        // send it
        client.send(JSON.stringify({result}))
    })
}

// call broadcastUpdate every 0.1s
setInterval(broadcastUpdate, 100);
