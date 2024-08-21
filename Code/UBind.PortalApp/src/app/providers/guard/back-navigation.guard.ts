import { Injectable } from '@angular/core';
import { CanDeactivate, NavigationEnd, Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';
import { filter } from 'rxjs/operators';

/**
 * Export Back navigation guard class
 * Navigation guard to prevent usage of browser's back button
 */
@Injectable({ providedIn: 'root' })
export class BackNavigationGuard implements CanDeactivate<any> {

    private browserBackTriggered: boolean = false;
    public constructor(
        location: LocationStrategy,
        private router: Router,
    ) {
        location.onPopState(() => {
            this.browserBackTriggered = true;
            return false;
        });

        this.onNavigationEndResetBrowserBackTriggered();
    }

    private onNavigationEndResetBrowserBackTriggered(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof NavigationEnd),
        ).subscribe(() => this.browserBackTriggered = false);
    }

    public canDeactivate(component: any): boolean {
        if (this.browserBackTriggered) {
            this.browserBackTriggered = true;
            history.pushState(null, null, location.href);
            return false;
        }
        return true;
    }
}
