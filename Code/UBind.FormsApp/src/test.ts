// This file is required by karma.conf.js and loads recursively all the .spec and framework files

// import 'zone.js/testing'; // leave this commented out otherwise you get ProxyZone errors.
import 'zone.js/dist/long-stack-trace-zone';
import 'zone.js/dist/proxy';
import 'zone.js/dist/sync-test';
import 'zone.js/dist/jasmine-patch';
import 'zone.js/dist/async-test';
import 'zone.js/dist/fake-async-test';

import { getTestBed } from '@angular/core/testing';
import {
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

import { HttpClientModule } from '@angular/common/http';

declare const require: any;

// First, initialize the Angular testing environment.
getTestBed().initTestEnvironment(
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting(), {
        teardown: { destroyAfterEach: false },
    },
);

// Maybe this could go in specific tests?
getTestBed().configureTestingModule({
    imports: [
        HttpClientModule,
    ],
});

// UB-11591 - set the default timeout interval to 10000ms 
// This will override the default timeout of 5000ms
// eslint-disable-next-line no-undef
jasmine.DEFAULT_TIMEOUT_INTERVAL = 10000;

// Then we find all the tests.
const context: any = require.context('./', true, /\.spec\.ts$/);
// And load the modules.
context.keys().map(context);
