import { Component, OnInit, ElementRef } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { CheckboxField } from '../checkbox/checkbox.field';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { ToggleFieldConfiguration } from '@app/resource-models/configuration/fields/toggle-field.configuration';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { takeUntil } from 'rxjs/operators';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export toggle field component class.
 * TODO: Write a better class header: toggle field functions.
 */
@Component({
    selector: '' + FieldSelector.Toggle,
    templateUrl: './toggle.field.html',
})

export class ToggleField extends CheckboxField implements OnInit {

    public useIcon: boolean = false;
    public toggleFieldConfiguration: ToggleFieldConfiguration;

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
            elementRef,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry);
        this.fieldType = FieldType.Toggle;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.toggleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.useIcon = this.toggleFieldConfiguration.icon != null;
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.toggleFieldConfiguration = <ToggleFieldConfiguration>configs.new;
                this.useIcon = this.toggleFieldConfiguration.icon != null;
            });
    }

    public handleKeyUp(event: any): void {
        this.toggleValue();
        this.onKeyDown(event);
    }
}
