import { Routes } from "@angular/router";
import { Permission } from "@app/helpers/permissions.helper";
import { UserType } from "@app/models/user-type.enum";
import { AuthenticationGuard } from "../providers/guard/authentication.guard";
import { PermissionGuard } from "../providers/guard/permission.guard";
import { RoutePaths } from "./route-path";
import { ErrorModule } from "../pages/error/error.module";
import { LoginModule } from "@app/pages/login/login.module";
import { CreateAccountModule } from "@app/pages/create-account/create-account.module";
import { LogoutModule } from "@app/pages/logout/logout.module";
import { ActivateAccountModule } from "@app/pages/activation/activate-account.module";
import { AgentHomeModule } from "@app/pages/home/agent-home.module";
import { CustomerHomeModule } from "@app/pages/home/customer-home.module";
import { MasterHomeModule } from "@app/pages/home/master-home.module";
import { AccountModule } from "@app/pages/account/account.module";
import { CustomerAccountModule } from "@app/pages/account/customer-account.module";
import { ClaimModule } from "@app/pages/claim/claim.module";
import { CustomerClaimModule } from "@app/pages/claim/customer-claim.module";
import { CustomerModule } from "@app/pages/customer/customer.module";
import { PolicyModule } from "@app/pages/policy/policy.module";
import { CustomerPolicyModule } from "@app/pages/policy/customer-policy.module";
import { ProductModule } from "@app/pages/product/product.module";
import { QuoteModule } from "@app/pages/quote/quote.module";
import { CustomerQuoteModule } from "@app/pages/quote/customer-quote.module";
import { TenantModule } from "@app/pages/tenant/tenant.module";
import { UserModule } from "@app/pages/user/user.module";
import { MessageModule } from "@app/pages/message/message.module";
import { CustomerMessageModule } from "@app/pages/message/customer-message.module";
import { MasterMessageModule } from "@app/pages/message/master-message.module";
import { ReportModule } from "@app/pages/report/report.module";
import { EditEmailTemplateModule } from "@app/pages/edit-email-template/edit-email-template.module";
import { RoleModule } from "@app/pages/role/role.module";
import { OrganisationModule } from "@app/pages/organisation/organisation.module";
import { PortalModule } from "@app/pages/portal/portal.module";
import { BlankModule } from "@app/pages/blank/blank.module";
import { SequentialRouteGuardRunner } from "@app/providers/guard/sequential-route-guard-runner";

/* An array that represents the uBind portal configuration that is used in app config service and app routing.
 * Reason of separation from 'app-routing.module.ts' is to make its definition on routing children during runtime.
 */
const routeChildren: Routes = [
    {
        path: RoutePaths.base,
        redirectTo: RoutePaths.login,
        pathMatch: 'full',
    },
    {
        path: RoutePaths.error,
        loadChildren: () => ErrorModule,
    },
    {
        path: RoutePaths.login,
        loadChildren: () => LoginModule,
        data: {
            menuDisabled: true,
        },
    },
    {
        path: RoutePaths.createAccount,
        loadChildren: () => CreateAccountModule,
        data: {
            menuDisabled: true,
        },
    },
    {
        path: RoutePaths.logout,
        loadChildren: () => LogoutModule,
        data: {
            menuDisabled: true,
        },
    },
    {
        path: RoutePaths.activate,
        loadChildren: () => ActivateAccountModule,
    },
    {
        path: RoutePaths.home,
        loadChildren: () => AgentHomeModule,
        data: {
            userTypes: [UserType.Client],
        },
    },
    {
        path: RoutePaths.myHome,
        loadChildren: () => CustomerHomeModule,
        data: {
            userTypes: [UserType.Customer],
        },
    },
    {
        path: RoutePaths.masterHome,
        loadChildren: () => MasterHomeModule,
        data: {
            userTypes: [UserType.Master],
        },
    },
    {
        path: RoutePaths.account,
        loadChildren: () => AccountModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewMyAccount],
            userTypes: [UserType.Client],
        },
    },
    {
        path: RoutePaths.myAccount,
        loadChildren: () => CustomerAccountModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewMyAccount],
            userTypes: [UserType.Customer],
        },
    },
    {
        path: RoutePaths.claim,
        loadChildren: () => ClaimModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
        },
    },
    {
        path: RoutePaths.myClaims,
        loadChildren: () => CustomerClaimModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewClaims],
        },
    },
    {
        path: RoutePaths.customer,
        loadChildren: () => CustomerModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
        },
    },
    {
        path: RoutePaths.policy,
        loadChildren: () => PolicyModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
        },
    },
    {
        path: RoutePaths.myPolicies,
        loadChildren: () => CustomerPolicyModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewPolicies],
        },
    },
    {
        path: RoutePaths.product,
        loadChildren: () => ProductModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewProducts],
        },
    },
    {
        path: RoutePaths.quote,
        loadChildren: () => QuoteModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions:
                [Permission.ViewQuotes,
                    Permission.ViewAllQuotes,
                    Permission.ViewAllQuotesFromAllOrganisations],
        },
    },
    {
        path: RoutePaths.myQuotes,
        loadChildren: () => CustomerQuoteModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewQuotes],
        },
    },
    {
        path: RoutePaths.tenant,
        loadChildren: () => TenantModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewTenants],
        },
    },
    {
        path: RoutePaths.user,
        loadChildren: () => UserModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations],
        },
    },
    {
        path: RoutePaths.message,
        loadChildren: () => MessageModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
        },
    },
    {
        path: RoutePaths.myMessage,
        loadChildren: () => CustomerMessageModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
        },
    },
    {
        path: RoutePaths.masterMessage,
        loadChildren: () => MasterMessageModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
        },
    },
    {
        path: RoutePaths.report,
        loadChildren: () => ReportModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewReports],
        },
    },
    {
        path: RoutePaths.emailTemplate,
        loadChildren: () => EditEmailTemplateModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHaveOneOfPermissions:
                [Permission.ManagePortals,
                    Permission.ManageTenants,
                    Permission.ManageProducts],
        },
    },
    {
        path: RoutePaths.role,
        loadChildren: () => RoleModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewRoles],
        },
    },
    {
        path: RoutePaths.organisation,
        loadChildren: () => OrganisationModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewOrganisations],
        },
    },
    {
        path: RoutePaths.portal,
        loadChildren: () => PortalModule,
        canActivate: [SequentialRouteGuardRunner],
        data: {
            sequentialGuards: [AuthenticationGuard, PermissionGuard],
            mustHavePermissions: [Permission.ViewPortals],
        },
    },
];

export const routes: Routes = [
    {
        path: ':portalTenantAlias',
        children: routeChildren,
    },
    {
        path: ':portalTenantAlias/:portalOrganisationAlias',
        children: routeChildren,
    },
    {
        path: 'default',
        loadChildren: () => BlankModule,
    },
    {
        path: 'error',
        loadChildren: () => ErrorModule,
    },
    {
        path: '**',
        loadChildren: () => ErrorModule,
    },
];
