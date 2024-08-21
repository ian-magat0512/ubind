import { Observable, Subject } from "rxjs";

/**
 * Tracks the validity of a something by counting the number of invalid children.
 * It sets this to valid when there are no invalid children.
 * Publishes the validity changes using an observable.
 */
export class ChildrenValidityTracker {

    public valid: boolean = true;
    public invalidChildCount: number = 0;
    private validSubject: Subject<boolean> = new Subject<boolean>();
    public validObservable: Observable<boolean> = this.validSubject.asObservable();

    public constructor(
    ) {
    }

    public onChildValidityChange(debugIdentifier: string, childValid: boolean): void {
        let oldValid: boolean = this.valid;
        this.invalidChildCount += childValid ? -1 : 1;
        if (this.invalidChildCount < 0) {
            throw new Error(`Somehow we got an invalid child count of `
                + `less than one (${this.invalidChildCount}) for widget `
                + `"${debugIdentifier}". This is a logic error with the application, and should `
                + `be reported to the developers.`);
        }
        this.valid = this.invalidChildCount == 0;
        if (this.valid != oldValid) {
            this.validSubject.next(this.valid);
        }
    }
}
