module.exports = {
    apps: [
        {
            name: "UDP",
            script: "/home/dps/jen/source-codes/agv-laundry-bluetooth-system/nodejs/udp-protocol/index.js",
            env: {
                NODE_ENV: "development"
            },
            env_production: {
                NODE_ENV: "production"
            },
            instances: 1,
            exec_mode: "fork"
        },
	{
            name: "watchdog-UDP",
            script: "/home/dps/jen/source-codes/drop-watchdog/index.js",
            env: {
                NODE_ENV: "development"
            },
            env_production: {
                NODE_ENV: "production"
            },
            instances: 1,
            exec_mode: "fork"
        }

    ]
}
