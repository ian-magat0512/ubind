import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { CustomerHomePage } from './customer-home/customer-home.page';
import { AuthenticationGuard } from '@app/providers/guard/authentication.guard';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CustomerPolicyCardComponent } from './customer-home/customer-policy-card/customer-policy-card.component';
import { CustomerClaimCardComponent } from './customer-home/customer-claim-card/customer-claim-card.component';
import { CustomerPolicyForRenewalCardComponent }
    from './customer-home/customer-policy-for-renewal-card/customer-policy-for-renewal-card.component';
import { CustomerQuoteCardComponent } from './customer-home/customer-quote-card/customer-quote-card.component';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: CustomerHomePage,
        canActivate: [EnvironmentGuard, AuthenticationGuard],
    },
];

/**
 * Export customer home module class.
 * TODO: Write a better class header: Ng Module declarations
 * of customer home.
 */
@NgModule({
    declarations: [CustomerHomePage,
        CustomerPolicyCardComponent,
        CustomerClaimCardComponent,
        CustomerPolicyForRenewalCardComponent,
        CustomerQuoteCardComponent],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
    exports: [CustomerPolicyCardComponent,
        CustomerClaimCardComponent,
        CustomerPolicyForRenewalCardComponent,
        CustomerQuoteCardComponent],
})
export class CustomerHomeModule { }
