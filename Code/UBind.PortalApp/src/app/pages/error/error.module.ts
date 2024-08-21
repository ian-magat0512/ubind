import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { ErrorPage } from './error.page';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: ErrorPage,
    },
];

/**
 * Export error module class.
 * TODO: Write a better class header: Ng module declarations of Error.
 */
@NgModule({
    declarations: [
        ErrorPage,
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routes),
    ],
})
export class ErrorModule { }
