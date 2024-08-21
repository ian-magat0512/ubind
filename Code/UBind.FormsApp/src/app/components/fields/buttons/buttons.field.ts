
import { Component } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { RadioField } from '../radio/radio.field';
import { CalculationService } from '@app/services/calculation.service';
import { WebhookService } from '@app/services/webhook.service';
import { DomSanitizer } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { EventService } from '@app/services/event.service';
import { ConfigService } from '@app/services/config.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export buttons field component class.
 * TODO: Write a better class header: buttons field function.
 */
@Component({
    selector: '' + FieldSelector.Buttons,
    templateUrl: './buttons.field.html',
    styleUrls: ['./buttons.field.scss'],
})
export class ButtonsField extends RadioField {

    public constructor(
        formService: FormService,
        webhookService: WebhookService,
        workflowService: WorkflowService,
        expressionDependencies: ExpressionDependencies,
        sanitizer: DomSanitizer,
        httpClient: HttpClient,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        configService: ConfigService,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            webhookService,
            workflowService,
            expressionDependencies,
            sanitizer,
            httpClient,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            configService,
            fieldEventLogRegistry,
        );
        this.fieldType = FieldType.Buttons;
    }
}
