import { Observable, Subject } from "rxjs";
import { IconLibrary } from "./icon-library.enum";

/**
 * This is the details for action buttons to show per portal pages.
 */
export class ActionButton {

    private label: string;
    private icon: string;
    private iconLibrary: IconLibrary;
    private actionName: string;
    private isPrimary: boolean;
    private hasActionLabel: boolean;
    private sortOrder: number;
    private clickEventSubject: Subject<Event>;

    private constructor(
        label: string,
        icon: string,
        iconLibrary: IconLibrary,
        actionName: string,
        isPrimary: boolean = false,
        hasActionLabel: boolean = false,
        sortOrder: number = null,
    ) {
        this.label = label;
        this.icon = icon;
        this.iconLibrary = iconLibrary || IconLibrary.IonicV4;
        this.actionName = actionName;
        this.isPrimary = isPrimary;
        this.hasActionLabel = hasActionLabel;
        this.sortOrder = sortOrder;
        this.clickEventSubject = new Subject<Event>();
    }

    public static createActionButton(
        label: string,
        icon: string,
        iconLibrary: IconLibrary,
        isPrimary: boolean,
        actionName: string,
        hasActionLabel: boolean,
        callback: () => Promise<void> | void,
        sortOrder: number = null,
    ): ActionButton {
        let actionButton: ActionButton = new ActionButton(
            label,
            icon,
            iconLibrary,
            actionName,
            isPrimary,
            hasActionLabel,
            sortOrder,
        );

        actionButton.ClickEventObservable.subscribe((event: any) => {
            if (callback) {
                if (event) {
                    callback();
                }
            }
        });

        return actionButton;
    }

    public get Label(): string {
        return this.label;
    }

    public get Icon(): string {
        return this.icon;
    }

    public get IconLibrary(): IconLibrary {
        return this.iconLibrary;
    }

    public get ActionName(): string {
        return this.actionName;
    }

    public get IsPrimary(): boolean {
        return this.isPrimary;
    }

    public get SortOrder(): number {
        return this.sortOrder;
    }

    public get HasActionLabel(): boolean {
        return this.hasActionLabel;
    }

    public set HasActionLabel(hasActionLabel: boolean) {
        this.hasActionLabel = hasActionLabel;
    }

    public onClick($event: Event): void {
        this.clickEventSubject.next($event);
    }

    public get ClickEventObservable(): Observable<Event> {
        return this.clickEventSubject;
    }
}
