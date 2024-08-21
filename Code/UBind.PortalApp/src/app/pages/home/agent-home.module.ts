import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { AgentHomePage } from './agent-home/agent-home.page';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { AuthenticationGuard } from '@app/providers/guard/authentication.guard';
import { WelcomeComponent } from '@app/components/welcome/welcome.component';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: AgentHomePage,
        canActivate: [AuthenticationGuard],
    },
];

/**
 * Export home module class.
 * This class manage Ng Module declaration of Home.
 */
@NgModule({
    declarations: [
        AgentHomePage,
        WelcomeComponent,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class AgentHomeModule { }
