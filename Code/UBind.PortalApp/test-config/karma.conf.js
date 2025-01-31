var webpackConfig = require('./webpack.test.js');

module.exports = function (config) {
    var _config = {
        basePath: '../',

        frameworks: ['jasmine'],

        files: [
            {
                pattern: './test-config/karma-test-shim.js',
                watched: true
            },
            {
                pattern: './src/assets/**/*',
                watched: false,
                included: false,
                served: true,
                nocache: false
            }
        ],

        proxies: {
            '/assets/': '/base/src/assets/'
        },

        preprocessors: {
            './test-config/karma-test-shim.js': ['webpack', 'sourcemap']
        },

        webpack: webpackConfig,

        webpackMiddleware: {
            stats: 'errors-only'
        },

        webpackServer: {
            noInfo: true
        },

        browserConsoleLogOptions: {
            level: '',
            format: '%b %T: %m',
            terminal: true
        },

        coverageIstanbulReporter: {
            reports: ['html', 'lcovonly'],
            fixWebpackSourcePaths: true
        },

        client: {
            captureConsole: true,
            mocha: {
                bail: true
            }
        },
        reporters: config.coverage ? ['kjhtml', 'dots', 'coverage-istanbul'] : ['kjhtml', 'dots'],
        port: 9876,
        colors: true,
        logLevel: config.LOG_INFO,
        autoWatch: true,
        browsers: ['ChromeDebugging'],
        singleRun: false,
        customLaunchers: {
            ChromeDebugging: {
                base: "Chrome",
                flags: ['--remote-debugging-port=9333']
            }
        }
    };

    config.set(_config);
};
