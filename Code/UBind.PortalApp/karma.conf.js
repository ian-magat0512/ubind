// Karma configuration file, see link for more information
// https://karma-runner.github.io/1.0/config/configuration-file.html

module.exports = function (config) {
    config.set({
        basePath: '',
        frameworks: ['jasmine', '@angular-devkit/build-angular'],
        plugins: [
            'karma-jasmine',
            'karma-chrome-launcher',
            'karma-jasmine-html-reporter',
            'karma-coverage-istanbul-reporter',
            '@angular-devkit/build-angular/plugins/karma',
            'karma-junit-reporter',
        ],
        client: {
            clearContext: false // leave Jasmine Spec Runner output visible in browser
        },
        coverageIstanbulReporter: {
            dir: require('path').join(__dirname, '../coverage'),
            reports: ['html', 'lcovonly'],
            fixWebpackSourcePaths: true
        },
        reporters: ['progress', 'kjhtml', 'junit'],
        port: 9876,
        colors: true,
        logLevel: config.LOG_INFO,
        autoWatch: true,
        browsers: ['Chrome'],
        customLaunchers: {
            ChromeHeadlessCI: {
                base: 'ChromeHeadless',
                flags: [
                    '--no-sandbox',
                    "--lang=en-AU",
                ]
            }
        },
        singleRun: false,

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
    });
};
