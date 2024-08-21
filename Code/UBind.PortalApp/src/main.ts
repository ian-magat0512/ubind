import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

import browserUpdate from 'browser-update';

if (environment.production) {
    enableProdMode();
}

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

platformBrowserDynamic().bootstrapModule(AppModule)
    .catch((err: any) => console.log(err));
