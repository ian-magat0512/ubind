import { Component, ViewChild, ViewContainerRef, OnInit, OnDestroy, HostBinding } from '@angular/core';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { Wrapper } from '../wrapper';

/**
 * Export anchor wrapper component class.
 * This class manage anchor wrapper functions.
 */
@Component({
    selector: 'anchor-wrapper',
    templateUrl: './anchor.wrapper.html',
})

export class AnchorWrapper extends Wrapper implements OnInit, OnDestroy {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    @HostBinding('class')
    public classes: string = 'field-anchor';

    @HostBinding('id')
    public htmlId: string;

    /**
     *
     */
    public constructor(private cssIdentifierPipe: CssIdentifierPipe) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.classes += ` key-${this.key}`;
        this.htmlId = this.cssIdentifierPipe.transform('anchor-' + this.fieldInstance.fieldPath);
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }
}
