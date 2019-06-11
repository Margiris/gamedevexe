const WebSocket = require('ws')

// create new websocket server
const wss = new WebSocket.Server({port: 8000})

// empty object to store all players
var result = 3

// on new client connect
wss.on('connection', function connection (client) {
  // on new message recieved
  client.on('message', function incoming (data) {
    // get data from string
    var [requestingServerStart, port, portOffset] = data.toString().split('\t')

    var exec = require('child_process').exec;
    // stdout is a string containing the output of the command.
    exec('powershell (Get-Process -Id (Get-NetTCPConnection -LocalPort ' + port + ').OwningProcess).Name', function(err, stdout, stderr) {
        // if port is currently not occupied by any process or is in use by BioDude server
        if err !== '' || stdout === 'BioDude'
            result = 0
        else
            result = 1
    });

    if result === 0 && requestingServerStart
        exec('BioDude.exe ' + port), function(err, stdout, stderr)
  })
})

function broadcastUpdate () {
  // broadcast messages to all clients
  wss.clients.forEach(function each (client) {
    // filter disconnected clients
    if (client.readyState !== WebSocket.OPEN) return
    // send it
    client.send(JSON.stringify({result}))
  })
}

// call broadcastUpdate every 0.1s
setInterval(broadcastUpdate, 100)
