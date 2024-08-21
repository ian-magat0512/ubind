import { Component, OnInit } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from 'app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { PasswordFieldConfiguration } from '@app/resource-models/configuration/fields/password-field.configuration';
import { TextInputFormat } from '@app/models/text-input-format.enum';
import { EventService } from '@app/services/event.service';
import { KeyboardInputField } from "@app/components/fields/keyboard-Input-field";
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export password field component class.
 * This class manage password field functions.
 */
@Component({
    selector: '' + FieldSelector.Password,
    templateUrl: './password.field.html',
})

export class PasswordField extends KeyboardInputField implements OnInit {

    private passwordFieldConfiguration: PasswordFieldConfiguration;
    public textInputFormat: TextInputFormat;

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
        this.fieldType = FieldType.Password;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.passwordFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.textInputFormat = this.passwordFieldConfiguration.textInputFormat;
    }
}
