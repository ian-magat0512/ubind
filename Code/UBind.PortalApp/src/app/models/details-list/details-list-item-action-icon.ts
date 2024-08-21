import { Subject, Observable } from "rxjs";
import { Permission } from "@app/helpers/permissions.helper";

/**
 * Repesents an action with an icon, that can be triggered on a details list item.
 */
export class DetailsListItemActionIcon {
    private icon: Array<string>;
    private iconLibrary: Array<string>;
    public iconIndex: number = 0;
    public hasPopOver: boolean;
    private clickEventSubject: Subject<Event>;

    /**
     * A list of permissions that are required to show the action.
     * This gets enforced in the route gaurd "PermissionGuard".
     */
    public mustHavePermissions?: Array<Permission>;

    /**
     * A list of permissions whereby the user must have at least one of them to show the action.
     * This gets enforced in the route gaurd "PermissionGuard".
     */
    public mustHaveOneOfPermissions?: Array<Permission>;

    /**
     * A list of sets of permissions whereby the user must have a permission from each set to show the action.
     */
    public mustHaveOneOfEachSetOfPermissions?: Array<Array<Permission>>;

    public constructor(icon: Array<string>, iconLibrary: Array<string> = []) {
        this.icon = icon;
        this.iconLibrary = iconLibrary;
        this.clickEventSubject = new Subject<Event>();
    }

    public withPopOver(): DetailsListItemActionIcon {
        this.hasPopOver = true;
        return this;
    }

    public get Icon(): string {
        return this.icon.length > 0 ? this.icon.length == 1 ? this.icon[0] : this.icon[this.iconIndex] : "";
    }

    public get IconLibrary(): string {
        return this.iconLibrary.length > 0
            ? this.iconLibrary.length == 1
                ? this.iconLibrary[0]
                : this.iconLibrary[this.iconIndex]
            : "";
    }

    public onClick($event: Event): void {
        this.clickEventSubject.next($event);
    }

    public get ClickEventObservable(): Observable<Event> {
        return this.clickEventSubject;
    }
}
