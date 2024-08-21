import { Subject, Observable } from "rxjs";
import { Permission } from "@app/helpers/permissions.helper";

/**
 * Export details list item action icon class.
 * TODO: Write a better class header: function of detail list actions.
 */
export class DetailsListItemActionIcon {
    private icon: Array<string>;
    public iconIndex: number = 0;
    public hasPopOver: boolean;
    private clickEventSubject: Subject<Event>;
    // checks this permission if using this action.
    public permission: Permission;

    public constructor(icon: Array<string>) {
        this.icon = icon;
        this.clickEventSubject = new Subject<Event>();
    }

    public withPopOver(): DetailsListItemActionIcon {
        this.hasPopOver = true;
        return this;
    }

    public get Icon(): string {
        return this.icon.length > 0 ? this.icon.length == 1 ? this.icon[0] : this.icon[this.iconIndex] : "";
    }

    public setPermission(permission: Permission): void {
        this.permission = permission;
    }

    public onClick($event: Event): void {
        this.clickEventSubject.next($event);
    }

    public get ClickEventObservable(): Observable<Event> {
        return this.clickEventSubject;
    }
}
