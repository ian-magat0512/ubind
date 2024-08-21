// Karma configuration
// Generated on Tue Sep 10 2019 17:21:18 GMT+1000 (AUS Eastern Standard Time)

module.exports = function (config) {
    config.set({
        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',

        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine', '@angular-devkit/build-angular', 'viewport'],
        plugins: [
            'karma-jasmine',
            'karma-chrome-launcher',
            'karma-jasmine-html-reporter',
            'karma-coverage-istanbul-reporter',
            '@angular-devkit/build-angular/plugins/karma',
            'karma-viewport',
            'karma-time-stats-reporter',
            'karma-junit-reporter',
            
            /* uncomment if you need to debug which test is failing */
            /* 'karma-spec-reporter', */
        ],

        client: {
            clearContext: false
        },
        coverageIstanbulReporter: {
            dir: require('path').join(__dirname, 'coverage'), reports: ['html', 'lcovonly'],
            fixWebpackSourcePaths: true
        },

        // list of files / patterns to load in the browser
        files: [
            {
                pattern: './src/assets/**/*',
                watched: false,
                included: false,
                served: true,
                nocache: false
            },
            {
                pattern: './src/app/services/**/*',
                watched: false,
                included: false,
                served: true,
                nocache: false
            }
        ],
        
        // list of files / patterns to exclude
        exclude: [
        ],

        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
        },

        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress', 'kjhtml', 'junit', 'time-stats'],

        /* Uncomment if you need to debug which test is failing */
        /* reporters: ['spec'], */

        // web server port
        port: 9876,

        // enable / disable colors in the output (reporters and logs)
        colors: true,

        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,

        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,

        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ['Chrome'],
        customLaunchers: {
            ChromeHeadlessCI: {
                base: 'ChromeHeadless',
                flags: [
                    '--no-sandbox',
                    '--lang=en-AU',
                ]
            }
        },

        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false,

        // Concurrency level
        // how many browser should be started simultaneous
        concurrency: Infinity,

        // This is config options for the reporter:
        timeStatsReporter: {
            reportTimeStats: true,           // Print Time Stats (histogram)
            binSize: 100,                    // Bin size for histogram (in milliseconds)
            slowThreshold: 500,              // The threshold for what is considered a slow test (in milliseconds).
                                            // This is also the max value for last bin histogram 
                                            // Note that this will automatically be rounded up to be evenly divisible by binSize
            reportSlowestTests: true,        // Print top slowest tests
            showSlowTestRankNumber: false,    // Displays rank number next to slow tests, e.g. `1) Slow Test`
            longestTestsCount: 10,            // Number of top slowest tests to list
                                            // Set to `Infinity` to show all slow tests. Useful in combination with `reportOnlyBeyondThreshold` as `true`
            reportOnlyBeyondThreshold: false // Only report tests that are slower than threshold
        },

        // the JUnit reporter is needed so that bamboo can pick up the build results and publish them
        junitReporter: {
            outputDir: 'test-reports', // results will be saved as $outputDir/$browserName.xml
            outputFile: undefined, // if included, results will be saved as $outputDir/$browserName/$outputFile
            suite: '', // suite will become the package name attribute in xml testsuite element
            useBrowserName: true, // add browser name to report and classes names
            nameFormatter: undefined, // function (browser, result) to customize the name attribute in xml testcase element
            classNameFormatter: undefined, // function (browser, result) to customize the classname attribute in xml testcase element
            properties: {}, // key value pair of properties to add to the <properties> section of the report
            xmlVersion: null // use '1' if reporting to be per SonarQube 6.2 XML format
        }
    })
}