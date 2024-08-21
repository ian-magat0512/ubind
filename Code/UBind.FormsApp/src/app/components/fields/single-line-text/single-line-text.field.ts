import { Component, OnInit } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { SingleLineTextFieldConfiguration,
} from '@app/resource-models/configuration/fields/single-line-text-field.configuration';
import { TextInputFormat } from '@app/models/text-input-format.enum';
import { EventService } from '@app/services/event.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { InputFieldWithMasking } from '../input-field-with-masking';
import { MaskPipe } from 'ngx-mask';

/**
 * Export single line text field component class
 * TODO: Write a better class header: single line text field functions.
 */
@Component({
    selector: '' + FieldSelector.SingleLineText,
    templateUrl: './single-line-text.field.html',
})

export class SingleLineTextField extends InputFieldWithMasking implements OnInit {

    /** 
     * if the field is sensitive, it means the characters will be masked by default
     */
    public sensitive: boolean = false;

    /**
     * When masked is true, instead of seeing the characters as you type, 
     * you will see a star or circle in place of each character
     */
    public masked: boolean = false;

    private singleLineTextFieldConfiguration: SingleLineTextFieldConfiguration;
    public textInputFormat: TextInputFormat;
    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected validator: ValidationService,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        fieldEventLogRegistry: FieldEventLogRegistry,
        protected myMaskPipe: MaskPipe,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            myMaskPipe);
        this.fieldType = FieldType.SingleLineText;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.singleLineTextFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.textInputFormat = this.singleLineTextFieldConfiguration.textInputFormat;
        this.sensitive = this.singleLineTextFieldConfiguration.sensitive
            || this.field.key == "creditCardNumber"
            || this.field.key == "creditCardCCV";
    }

    public toggleMask(event: Event): void {
        event.preventDefault();
        this.masked = !this.masked;
    }
}
