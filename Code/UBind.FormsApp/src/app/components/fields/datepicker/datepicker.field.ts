import { Component, ElementRef, OnInit, HostListener, AfterViewChecked, AfterViewInit } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { takeUntil } from 'rxjs/operators';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { EventService } from '@app/services/event.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { KeyboardInputField } from "@app/components/fields/keyboard-Input-field";
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

// eslint-disable-next-line no-unused-vars
declare let jquery: any;
declare let $: any;

// import * as $ from 'jquery';

/**
 * Export date picker field component class.
 * TODO: Write a better class header: datepicker functions.
 */
@Component({
    selector: '' + FieldSelector.DatePicker,
    templateUrl: './datepicker.field.html',
})
export class DatepickerField extends KeyboardInputField implements OnInit, AfterViewChecked, AfterViewInit {

    public readonly MinimumMediumScreenWidth: number = 768;

    protected nonNativeElementId: any;
    public nativeDateFieldValue: any;
    public isNative: any;
    public htmlId: string;
    private isSelectedFromPicker: boolean = false;

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected elementRef: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        private cssIdentifierPipe: CssIdentifierPipe,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry);
        this.fieldType = FieldType.DatePicker;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.onUpdateDisabledState();
        this.htmlId = this.cssIdentifierPipe.transform(this.fieldPath);
    }

    public ngAfterViewInit(): void {
        if (!this.nonNativeElementId) {
            // Use the fieldPath as the non-native element id since these version of jquery
            // associate the datepicker to the id of the element. This change will make the datepicker
            // update the correct field.
            this.nonNativeElementId = this.htmlId;

            $.datepicker.setDefaults(
                $.extend(
                    { 'dateFormat': 'dd/mm/yy' },
                    $.datepicker.regional['au'],
                ),
            );

            this.initializeDatepicker();
        }
    }

    public ngAfterViewChecked(): void {
        this.updateNativeDateElementValue(this.formControl.value);
    }

    @HostListener('load', ['$event'])
    public onLoad(event: any): void {
        this.isNative = window.innerWidth < this.MinimumMediumScreenWidth;
    }

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        const prevNativeValue: any = this.isNative;
        this.isNative = window.innerWidth < this.MinimumMediumScreenWidth;

        if (prevNativeValue && !this.isNative) {
            setTimeout(() => {
                this.initializeDatepicker();
            }, 50);
        }
    }

    protected onUpdateDisabledState(): void {
        if (this.disabledExpression) {
            this.disabledExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((disabled: boolean) => this.setDisabledAttribute(disabled));
        }
    }

    protected setDisabledAttribute(condition: boolean): void {
        let inputs: any = this.elementRef.nativeElement.getElementsByTagName('input') as HTMLCollection;
        if (condition) {
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            for (let i: number = 0; i < inputs.length; i++) {
                inputs[i].setAttribute('disabled', 'true');
            }
        } else {
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            for (let i: number = 0; i < inputs.length; i++) {
                inputs[i].removeAttribute('disabled');
            }
        }
    }

    protected updateNativeDateElementValue(value: string): void {
        let expression: RegExp = /^[0-9][0-9]\/[0-9][0-9]\/[0-9][0-9][0-9][0-9]$/;
        let stringValue: string = '';
        if (expression.test(value)) {
            let dateProperties: any = value.split('/');
            stringValue = dateProperties[2] + '-' + dateProperties[1] + '-' + dateProperties[0];
        }
        this.nativeDateFieldValue = stringValue;
    }

    public ngModelChange($event: any): void {
        let value: string = this.convertNativeDateValue($event);
        this.setValue(value);
    }

    /**
     * Converts the native data value to a standard ubind string format
     */
    private convertNativeDateValue(value: any): string {
        // eslint-disable-next-line no-useless-escape
        let expression: RegExp = /^[0-9][0-9][0-9][0-9]\-[0-9][0-9]\-[0-9][0-9]$/;
        let stringValue: string = '';
        if (expression.test(value)) {
            let dateProperties: any = value.split('-');
            stringValue = dateProperties[2] + '/' + dateProperties[1] + '/' + dateProperties[0];
        }
        return stringValue;
    }

    // This is a hack to trigger validation of the field after the value has
    // been changed by the jQuery datepicker. For some reason it doesn't trigger
    // by itself, and I can't work out a less 'hacky' way to make it work.
    public onChange(event: any): void {
        if (event && event['value'] != null) {
            if (this.isSelectedFromPicker) {
                event.srcElement.focus();
                this.resetIsSelectedFromPicker();
            }
            this.form.controls[this.fieldKey].setValue(event['value']);
            this.form.controls[this.fieldKey].markAsTouched();
            this.updateNativeDateElementValue(event['value']);
        } else {
            this.form.controls[this.fieldKey].setValue(this.formControl.value);
        }
        super.onChange(event);
    }

    public onClick(event: any): void {
        const datepicker: any = $('[id="' + this.nonNativeElementId + '"]');
        if (!datepicker.datepicker('widget').is(':visible')) {
            datepicker.datepicker('show');
        }
    }

    protected onStatusChange(status: string = null): void {
        // we need to delay the status change until the field has had time to update it's value
        setTimeout(() => super.onStatusChange(status), 0);
    }

    private resetIsSelectedFromPicker(): void {
        this.isSelectedFromPicker = false;
    }

    private initializeDatepicker(): void {
        if (!this.isNative) {
            $('[id="' + this.nonNativeElementId + '"]').datepicker({
                changeMonth: true,
                changeYear: true,
                yearRange: '-100:+10',
                onSelect: (value: any): void => {
                    this.isSelectedFromPicker = true;
                    $('.ui-datepicker a').removeAttr('href'); // fixes IE11 bug
                    const element: Element = document.querySelector('[id="' + this.nonNativeElementId + '"]');
                    const event: any = new CustomEvent('change');
                    event['value'] = value;
                    element.dispatchEvent(event);
                },
            });
        }
    }
}
