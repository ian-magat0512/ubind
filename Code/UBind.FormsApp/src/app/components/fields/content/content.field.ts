import { takeUntil } from 'rxjs/operators';
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
 * Export content field component class.
 * TODO: Write a better class header: content field component function.
 */
@Component({
    selector: '' + FieldSelector.Content,
    templateUrl: './content.field.html',
})

export class ContentField extends Field {

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

        this.fieldType = FieldType.Content;
    }

    protected initialiseField(): void {
        this.initialisationStarted = true;
        this.formControl['field'] = this;
        this.formControl.setValue('');
        this.formControl.markAsTouched();
        this.parentQuestionsWidget = this.form.root['questionSet'];

        // set the initial validity and ensure the question set knows it
        this.valid = this.formControl.valid;
        this.fieldValidSubject = this.expressionDependencies.expressionInputSubjectService.getFieldValidSubject(
            this.fieldPath,
            this.valid);
        if (!this.valid) {
            this.parentQuestionsWidget.childrenValidityTracker.onChildValidityChange(this.fieldKey, this.valid);
        }

        // listen to changes to the status (validity) of this field
        this.formControl.statusChanges
            .pipe(takeUntil(this.destroyed))
            .subscribe((newStatus: string) => {
                setTimeout(() => {
                    // we are using setTimeout here to introduce a slight delay to allow expressions to 
                    // update the other fields
                    this.onStatusChange(newStatus);
                }, 50);
            });

        this.initialised = true;
    }
}
