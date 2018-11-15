var path = require("path");

module.exports = {
    mode: "development",
    entry: "./FinTalkDemo.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js"
    },
    devServer: {
        contentBase: "./public",
        port: 8080,
        proxy: {
            '/api/*': {
                target: "http://localhost:5000",
                changeOrigin: true
            }
        }
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
};