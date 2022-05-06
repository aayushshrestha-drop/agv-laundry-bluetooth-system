const fs = require('fs');
const dgram = require('dgram');
const server = dgram.createSocket('udp4');

const protocol = require("./protocol");
const rabbitmq = require("./rabbitmq");
const config = require("./config");

setTimeout(() => {
    rabbitmq.connect();
    server.on('listening', function () {
        const address = server.address();
        console.log('UDP Server listening on ' + address.address + ":" + address.port);
    });
    server.on('message', function (message, remote) {
        try {
            const parsed = JSON.parse(message.toString());
            if (parsed.tagAddress.startsWith('2034')) {
                const msg = `${message.toString('hex').length} \n`;
                console.log(`${parsed.address} ---> ${parsed.tagAddress} | ${parsed.rssi} | ${parsed.batt}`)
                rabbitmq.sendMessage(JSON.stringify(parsed));
            }
        }
        catch (ex) {
            console.log(ex)
        }

    });
    server.bind(config.UDP_PORT, config.UDP_HOST);
}, 20000);

