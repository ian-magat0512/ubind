const path = require('path');

module.exports = {
    entry: './src/ubind-injector.ts',
    mode: 'production',
    devtool: 'source-map',
    module: {
        rules: [
            {
                test: /\.js$/,
                enforce: 'pre',
                use: ['source-map-loader'],
            },
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        // previously ../../UBind.Web/wwwroot/assets/ubind.js and now assets/ubind.js
        filename: 'assets/ubind.js',
        path: path.resolve(__dirname, 'dist'),
    },
};
