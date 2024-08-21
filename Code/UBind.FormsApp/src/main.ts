/* eslint-disable no-unused-vars */
import { ApplicationRef, enableProdMode, NgModuleRef } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

import browserUpdate from 'browser-update';
import { enableDebugTools } from '@angular/platform-browser';

const browserUpdateOptions: any = {
    required: {
        e: 17,
        f: 67,
        o: 55,
        s: 12,
        c: 67,
    },
};

browserUpdate(browserUpdateOptions);

// Always endable prod mode, otherwise large forms are too slow.
// if (environment.production) {
enableProdMode();
// }

platformBrowserDynamic().bootstrapModule(AppModule)
/** ****** THIS ALLOWS YOU TO DO PROFILING ****/
/*
  .then((moduleRef: NgModuleRef<AppModule>) => {
    const applicationRef: ApplicationRef = moduleRef.injector.get(ApplicationRef);
    const componentRef: any = applicationRef.components[0];
    // allows to run `ng.profiler.timeChangeDetection();`
    enableDebugTools(componentRef);
  })
  */
/** ****** END PROFILING ****/
    .catch((err: any) => console.log(err));
