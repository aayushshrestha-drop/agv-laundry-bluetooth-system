const amqp = require('amqplib');
const config = require("./config");
let CHANNEL;
const createChannel = (connection) => {
    connection.createChannel().then((channel) => {
        channel.assertQueue(config.AMQP_TOPIC, {
            durable: false
        });
        CHANNEL = channel;
    }).error((error1) => {
        if (error1) {
            throw error1;
        }
    });
}
const connect = () => {
    amqp.connect({
        protocol: 'amqp',
        hostname: config.AMQP_HOST,
        port: config.AMQP_PORT,
        username: config.AMQP_USERNAME,
        password: config.AMQP_PASSWORD,
        locale: 'en_US',
        frameMax: 0,
        heartbeat: 0,
        vhost: '/'
    }).then((connection) => {
        createChannel(connection);
    }).error((error0) => {
        if (error0) {
            throw error0;
        }
    });
}

const sendMessage = (msg) => {
    if (CHANNEL) {
        // CHANNEL.sendToQueue(config.AMQP_TOPIC, Buffer.from(msg));
        CHANNEL.publish(config.AMQP_EXCHANGE, '', Buffer.from(msg));
        console.log(" [x] Sent %s");
    }
}
module.exports = {
    connect: connect,
    sendMessage: sendMessage
};