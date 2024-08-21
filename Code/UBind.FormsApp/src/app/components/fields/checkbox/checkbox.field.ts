import { takeUntil } from 'rxjs/operators';
import { Component, OnInit, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { Field } from '../field';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { SafeHtml } from '@angular/platform-browser';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { StringHelper } from '@app/helpers/string.helper';
import { CheckboxFieldConfiguration } from '@app/resource-models/configuration/fields/checkbox-field.configuration';
import { EventService } from '@app/services/event.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * This component manages the checkbox field functions and layout.
 */
@Component({
    selector: '' + FieldSelector.Checkbox,
    templateUrl: './checkbox.field.html',
})

export class CheckboxField extends Field implements OnInit, OnDestroy {
    @ViewChild('tabbableElement', { static: true }) public tabbableElement: ElementRef;
    public labelSafeHtml: SafeHtml;
    private checkboxFieldConfiguration: CheckboxFieldConfiguration;

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected elementRef: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
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
        this.fieldType = FieldType.Checkbox;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.checkboxFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.onUpdateDisabledState();
        this.setupLabelExpression();
    }

    public ngOnDestroy(): void {
        this.checkboxFieldConfiguration = null;
        super.ngOnDestroy();
    }

    protected onUpdateDisabledState(): void {
        if (this.disabledExpression) {
            this.disabledExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((disabled: boolean) => this.setDisabledAttribute(disabled));
        }
    }

    public handleKeyUp(event: any): void {
        this.formControl.setValue(!this.formControl.value);
        this.keyPressSubject.next(event);
    }

    protected setDisabledAttribute(condition: boolean): void {
        let inputs: HTMLCollection = this.elementRef.nativeElement.getElementsByTagName('input') as HTMLCollection;
        if (condition) {
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            for (let i: number = 0; i < inputs.length; i++) {
                inputs[i].setAttribute('disabled', 'true');
                inputs[i].parentElement.classList.add("disabled");
            }
        } else {
            // eslint-disable-next-line @typescript-eslint/prefer-for-of
            for (let i: number = 0; i < inputs.length; i++) {
                inputs[i].removeAttribute('disabled');
                inputs[i].parentElement.classList.remove("disabled");
            }
        }
    }

    protected setupLabelExpression(): void {
        let valueTemplate: string = this.checkboxFieldConfiguration.placeholder;
        if (!StringHelper.isNullOrEmpty(valueTemplate)) {
            let labelExpression: TextWithExpressions = new TextWithExpressions(
                valueTemplate,
                this.expressionDependencies,
                this.fieldKey + ' checkbox label',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            labelExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => this.labelSafeHtml = result);
            labelExpression.triggerEvaluation();
        }
    }

    protected setFormControlValue(value: any): void {
        let checkboxValue: boolean = value == true || value == 'checked' ? true : false;
        this.formControl.setValue(checkboxValue);
    }

    /**
     * We need to convert the 'checked' value into a true/false to provide the real value
     * because a checkbox is actually true/false in the form model
     */
    protected getValueForExpressions(): any {
        if (this.isHidden()) {
            return '';
        }
        return this.formControl.value == 'checked' || this.formControl.value == true;
    }

    public get styles(): any {
        let styles: any = {};
        if (this.checkboxFieldConfiguration.containerClass && this.checkboxFieldConfiguration.containerCss) {
            let properties: Array<string> = this.checkboxFieldConfiguration.containerCss.split(';');
            for (let property of properties) {
                let firstColonPosition: number = property.indexOf(':');
                if (firstColonPosition && firstColonPosition < property.length - 1) {
                    let key: string = property.substring(0, firstColonPosition);
                    let value: string = property.substring(firstColonPosition + 1);
                    styles[key] = value;
                }
            }
        }
        if (this.checkboxFieldConfiguration.widgetCssWidth) {
            styles['width'] = this.checkboxFieldConfiguration.widgetCssWidth;
        }
        return styles;
    }

    public setFocusOnTabbableElement() {
        if (this.tabbableElement && this.tabbableElement.nativeElement) {
            this.tabbableElement.nativeElement.focus();
        }
    }
}
