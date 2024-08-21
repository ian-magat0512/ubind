import { Component } from '@angular/core';
import { Field } from '../field';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { EventService } from '@app/services/event.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export hidden field component class.
 * This class manage hidden field functions.
 */
@Component({
    selector: '' + FieldSelector.Hidden,
    templateUrl: './hidden.field.html',
})

export class HiddenField extends Field {

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
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
        this.fieldType = FieldType.Hidden;
    }
}
