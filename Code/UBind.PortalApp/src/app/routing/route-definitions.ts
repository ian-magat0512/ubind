import { PathRedirectGuard } from "@app/providers/guard/path-redirect.guard";
import { pageRoutes } from "./page-route-definitions";
import { PortalRedirectGuard } from "@app/providers/guard/portal-redirect.guard";
import { TypedRoutes } from "./typed-route";
import { SequentialRouteGuardRunner } from "@app/providers/guard/sequential-route-guard-runner";
import { PopulateNullOrganisationAliasGuard } from "@app/providers/guard/populate-null-organisation-alias.guard";

export const routes: TypedRoutes = [
    {
        path: ':portalTenantAlias/path',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: ':portalTenantAlias/:portalOrganisationAlias/path',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PopulateNullOrganisationAliasGuard, PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: ':portalTenantAlias/:portalOrganisationAlias/:portalAlias/path',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PopulateNullOrganisationAliasGuard, PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: ':portalTenantAlias',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: ':portalTenantAlias/:portalOrganisationAlias',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PopulateNullOrganisationAliasGuard, PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: ':portalTenantAlias/:portalOrganisationAlias/:portalAlias',
        children: pageRoutes,
        canActivate: [SequentialRouteGuardRunner],
        data: { sequentialGuards: [PopulateNullOrganisationAliasGuard, PortalRedirectGuard, PathRedirectGuard] },
    },
    {
        path: 'default',
        loadChildren: './pages/blank/blank.module#BlankModule',
    },
    {
        path: 'error',
        loadChildren: './pages/error/error.module#ErrorModule',
    },
    {
        path: '**',
        loadChildren: './pages/error/error.module#ErrorModule',
    },
];
