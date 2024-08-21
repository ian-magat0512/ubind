import { Injectable } from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { filter, map } from "rxjs/operators";

/**
 * This service is needed so you can register an action to be executed at a later stage.
 * Typically this is used for things like a customer quote association, which needs to happen
 * after the customer is redirected to the home page.
 */
@Injectable({ providedIn: 'root' })
export class ActionService {

    private actions: Array<any> = new Array<any>();

    public constructor(
        protected router: Router,
    ) {
        this.onNavigationEndExecuteActions();
    }

    public register(action: () => void): void {
        this.actions.push(action);
    }

    private executeAll(): void {
        this.actions.forEach((action: () => void) => action());
        this.actions = new Array<any>();
    }

    private onNavigationEndExecuteActions(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof NavigationEnd),
            map((event: any) => (<NavigationEnd>event).url),
        ).subscribe((url: string) => {
            this.executeAll();
        });
    }
}
