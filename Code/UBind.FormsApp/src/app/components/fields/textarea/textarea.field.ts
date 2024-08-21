import { Component, OnInit } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { EventService } from '@app/services/event.service';
import { KeyboardInputField } from "@app/components/fields/keyboard-Input-field";
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export textarea field component class.
 * TODO: Write a better class header: textarea field functions.
 */
@Component({
    selector: '' + FieldSelector.TextArea,
    templateUrl: './textarea.field.html',
})

export class TextareaField extends KeyboardInputField  implements OnInit {

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
        this.fieldType = FieldType.TextArea;
    }

}
