const POSITION = ['In Position', 'Out of Position'];
function parseMessage(message, remote) {
    let hex = message.toString('hex');
    let h = hex.substring(0, 2);
    let w = hex.substring(2, 4);
    let vendor_id = parseInt(`${h}${w}`);
    let version = parseInt(hex.substring(4, 6));
    let ap_mac = hex.substring(6, 18);
    let reserved_1 = hex.substring(18, 22);
    let number = hex.substring(22, 24);
    let tag_mac = hex.substring(24, 36);
    let timestamp = parseInt(hex.substring(36, 44), 16);
    let rssi = parseInt(hex.substring(44, 46), 16) - 256;
    let reserved_2 = parseInt(hex.substring(48, 50), 16);
    let batt = hex.length == 72 ? parseInt(hex.substring(hex.length - 2, hex.length), 16) : null;
    let uuid = hex.length == 112 ? hex.substring(hex.length - 42, hex.length - 10) : null;
    let major = hex.length == 112 ? parseInt(hex.substring(hex.length - 10, hex.length - 6), 16) : null;
    let minor = hex.length == 112 ? parseInt(hex.substring(hex.length - 6, hex.length - 2), 16) : null;

    return {
        vendor_id: vendor_id,
        version: version,
        ap_mac: ap_mac,
        reserved_1: reserved_1,
        number: number,
        tag_mac: tag_mac,
        timestamp: new Date(timestamp),
        rssi: rssi,
        batt: batt,
        reserved_2: POSITION[reserved_2],
        uuid: uuid,
        major: major,
        minor: minor
    }
}
module.exports = {
    parseMessage: parseMessage
};