const fs = require('fs');
const dgram = require('dgram');
const server = dgram.createSocket('udp4');

const protocol = require("./protocol");
const rabbitmq = require("./rabbitmq");
const config = require("./config");

setTimeout(() => {
    //rabbitmq.connect();
    server.on('listening', function () {
        const address = server.address();
        console.log('UDP Server listening on ' + address.address + ":" + address.port);
    });
    server.on('message', function (message, remote) {
        try {
            console.log(message.toString())
            // const parsed = protocol.parseMessage(message);
            // if (parsed.tag_mac.startsWith('2034')) {
            //     //console.log('---------------------------------------------------------------------------------------------');
            //     const msg = `${message.toString('hex').length} \n`;
            //     // console.log(message.toString('hex'));
            //     // console.log(msg);
            //     // console.log(parsed)
            //     // console.log(`${parsed.ap_mac} ---> ${parsed.tag_mac} | ${parsed.rssi}`)
            //     rabbitmq.sendMessage(JSON.stringify(parsed));
            //     // console.log('---------------------------------------------------------------------------------------------');
            // }
        }
        catch (ex) {
            console.log(ex)
        }

    });
    server.bind(config.UDP_PORT, config.UDP_HOST);
}, 20000);

