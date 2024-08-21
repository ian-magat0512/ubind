import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { Wrapper } from '../wrapper';

/**
 * Export validation wrapper component class.
 * TODO: Write a better class header: validation wrapper functions.
 */
@Component({
    selector: 'formly-wrapper-validation-messages',
    templateUrl: './validation.wrapper.html',
})

export class ValidationWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    public validationId: string;

    public ngOnInit(): void {
        this.validationId = this.field.id + '-message';
    }
}
