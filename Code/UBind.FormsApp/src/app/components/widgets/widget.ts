import { OnInit, OnDestroy, Directive } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Base class for a widget, which is a component rendered as part of the form which is not an input field.
 */
@Directive()

export abstract class Widget implements OnInit, OnDestroy {

    protected destroyed: Subject<void> = new Subject<void>();

    protected constructor() {
    }

    // eslint-disable-next-line @angular-eslint/no-empty-lifecycle-method
    public ngOnInit(): void {
        // This needs to be here so that it can be called consistently by subclasses
    }

    public ngOnDestroy(): void {
        this.destroyed?.next();
        this.destroyed?.complete();
        this.destroyExpressions();
    }

    protected destroyExpressions(): void {
    }
}
