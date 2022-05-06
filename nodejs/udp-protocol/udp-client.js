const dgram = require('dgram');
const server = dgram.createSocket('udp4');


server.send('test', 5555, '54.255.98.13', (err) => {
    if(err) console.log(err);
    else console.log("Message Sent")
})