module.exports = {
    apps: [
        {
            name: "UDP",
            script: "/home/ubuntu/source-codes/location-engine/node-js/huawei-protocol/index.js",
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
