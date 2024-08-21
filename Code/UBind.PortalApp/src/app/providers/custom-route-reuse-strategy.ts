import { ActivatedRouteSnapshot, RouteReuseStrategy, DetachedRouteHandle } from '@angular/router';
import { Injectable } from '@angular/core';

/**
 * This class is needed to fix a bug with angular where the activatedRoute returns the previous route's data, 
 * not the future route. 
 * This is because by default angular is re-using the route somehow.
 */
@Injectable()
export class CustomRouteReuseStrategy implements RouteReuseStrategy {
    public shouldDetach(route: ActivatedRouteSnapshot): boolean {
        return false;
    }

    public store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle | null): void {
    }

    public shouldAttach(route: ActivatedRouteSnapshot): boolean {
        return false;
    }

    public retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
        return null;
    }

    public shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
        return false;
    }
}
