import { NgModule } from '@angular/core';
import { CreateAccountComponent } from './create-account.component';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { RouterModule } from '@angular/router';
import { LocalAccountGuard } from '@app/providers/guard/local-account.guard';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: CreateAccountComponent,
        canActivate: [LocalAccountGuard],
    },
];

/**
 * Create account module.
 */
@NgModule({
    declarations: [
        CreateAccountComponent,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class CreateAccountModule { }
