import { Injectable, Injector, Type } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { TypedRouteData } from '@app/routing/typed-route-data';
import { Observable } from 'rxjs';

/**
 * A route guard that instantiates and runs a sequence of other route guards, in order, regardless
 * of whether they return a Promise or an Observable.
 * This is needed because Angular's router will run route guards concurrently if they return a Promise or
 * an Observable, which is not always desirable. This presents problems in our typicall use case of having
 * an AuthenticationGuard followed by a PermissionGuard. If the AuthenticationGuard sets up the user's
 * authentication state, and the PermissionGuard then checks the user's permissions, the PermissionGuard
 * will run before the AuthenticationGuard has finished setting up the user's authentication state.
 * This guard will run the guards in sequence, and stop processing further guards if any guard returns
 * false or a UrlTree.
 */
@Injectable({
    providedIn: 'root',
})
export class SequentialRouteGuardRunner implements CanActivate {

    public constructor(private injector: Injector) { }

    public async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
        const routeData: TypedRouteData = route.data;
        const guards: Array<Type<CanActivate>> = routeData.sequentialGuards || [];
        for (const guardClass of guards) {
            const guard: CanActivate = this.injector.get(guardClass);
            const result: boolean | UrlTree = await this.resolveGuard(guard, route, state);
            if (result === false || result instanceof UrlTree) {
                return result;
            }
        }
        return true; // All guards passed
    }

    private async resolveGuard(
        guard: CanActivate,
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        const result: Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree
            = guard.canActivate(route, state);
        if (result instanceof Observable) {
            return result.toPromise();
        }
        return result; // Handles boolean, UrlTree, and Promise<boolean | UrlTree>
    }
}
