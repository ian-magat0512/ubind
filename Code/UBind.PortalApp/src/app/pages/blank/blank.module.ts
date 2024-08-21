import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { BlankSharedComponentModule } from './blank-shared-component.module';
import { BlankPage } from './blank.page';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: BlankPage,
    },
];

/**
 * Export blank module class
 */
@NgModule({
    declarations: [],
    imports: [
        SharedModule,
        BlankSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class BlankModule { }
