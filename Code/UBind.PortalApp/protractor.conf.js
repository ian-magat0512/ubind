// Protractor configuration file, see link for more information
// https://github.com/angular/protractor/blob/master/lib/config.ts

const { SpecReporter } = require('jasmine-spec-reporter');

exports.config = {
    allScriptsTimeout: 300000,

    seleniumAddress: 'http://127.0.0.1:4444/wd/hub',
    //   capabilities: 
    //      {
    //          'browserName': 'chrome',
    //          specs: [
    //              './e2e/src/**/_full-integration-workflow-test.e2e.spec.ts',
    //          ],
    //          chromeOptions: {
    //              //args: ['--headless', '--window-size=1355x1060']
    //          }
    //      },
    multiCapabilities: [
        // {
        //     'browserName': 'chrome',
        //     specs: [
        //         './e2e/src/**/_full-integration-workflow-test.e2e.spec.ts',
        //         //'./e2e/src/**/_workflow-test.e2e.spec.ts',
        //     ],
        //     chromeOptions: {
        //         'mobileEmulation': {
        //             'deviceName': 'iPhone X'
        //         }
        //         //args: ['--headless', '--window-size=1355x1060']
        //     }
        // },
        // {
        //     'browserName': 'chrome',
        //     specs: [
        //         './e2e/src/**/_full-integration-workflow-test.e2e.spec.ts',
        //         //'./e2e/src/**/_workflow-test.e2e.spec.ts',
        //     ],
        //     chromeOptions: {
        //         //args: ['--headless', '--window-size=1355x1060']
        //     }
        // },

        // // // // MOBILE VIEW
        {
            'browserName': 'chrome',
            "collection": "demosLatitudeMotorLoanTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "australianReliancePropertyClubTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "depositAssureConciergeTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "leaseAssetFinanceMotorpacTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "mgaPetsTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'Apple iPhone X'
                },
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "insureMyPromoPromoInABoxTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                'mobileEmulation': {
                    'deviceName': 'iPhone X'
                },
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        // // // // // DESKTOP VIEW
        {
            'browserName': 'chrome',
            "collection": "demosLatitudeMotorLoanTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "australianReliancePropertyClubTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                // args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "depositAssureConciergeTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "leaseAssetFinanceMotorpacTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                // args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "mgaPetsTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
        {
            'browserName': 'chrome',
            "collection": "insureMyPromoPromoInABoxTest",
            specs: [
                './e2e/src/**/_workflow-test.e2e.spec.ts',
            ],
            chromeOptions: {
                //args: ['--headless', '--window-size=1355x1060']
            }
        },
    ],
    maxSessions: 2,
    directConnect: true,
    baseUrl: 'http://localhost:8100/',
    framework: 'jasmine',
    jasmineNodeOpts: {
        showColors: true,
        defaultTimeoutInterval: 30000,
        print: function () { }
    },
    onPrepare() {
        require('ts-node').register({
            project: './e2e/tsconfig.e2e.json'
        });
        jasmine.getEnv().addReporter(new SpecReporter({ spec: { displayStacktrace: true } }));

        browser.getProcessedConfig().then(function (config) {
            console.log(config.capabilities.collection);
            browser.collection = config.capabilities.collection;
        });
    },
    beforeLaunch: function () {
    }
};