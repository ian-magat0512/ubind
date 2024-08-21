import { OnDestroy, OnInit, ViewChild, ViewContainerRef, Directive } from "@angular/core";
import { FieldWrapper } from "@ngx-formly/core";
import { Subject } from "rxjs";
import { Field } from "../fields/field";

/**
 * Export wrapper abstract class.
 * TODO: Write a better class header: wrapper fieldwarpper functions.
 */
@Directive()

export abstract class Wrapper extends FieldWrapper implements OnInit, OnDestroy {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    protected destroyed: Subject<void> = new Subject<void>();
    public fieldKey: string;
    public fieldInstance: Field;

    public constructor() {
        super();
    }

    public ngOnInit(): void {
        this.fieldKey = <string>this.key;
        this.fieldInstance = this.field.templateOptions.componentRef;
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        this.destroyExpressions();
    }

    protected destroyExpressions(): void {
    }
}
