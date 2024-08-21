import { Component, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { TextWithExpressions } from "@app/expressions/text-with-expressions";
import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import { VisibleFieldConfiguration } from "@app/resource-models/configuration/fields/visible-field.configuration";
import { EventService } from "@app/services/event.service";
import { takeUntil } from "rxjs/operators";
import { Wrapper } from "../wrapper";

/**
 * This wrapper was created when upgrading from formly v4 to v5.
 * They removed the deprecated description wrapper, so we need to add our own
 * custom wrapper to replace it. The description wrapper is for rendering help text
 * underneath the field.
 */
@Component({
    selector: 'formly-wrapper-description',
    templateUrl: './description.wrapper.html',
})
export class DescriptionWrapper extends Wrapper implements OnInit {

    public helpMessage: string;
    private textExpression: TextWithExpressions;
    private textExpressionSource: string;
    private visibleFieldConfiguration: VisibleFieldConfiguration;

    public constructor(
        private expressionDependencies: ExpressionDependencies,
        protected sanitizer: DomSanitizer,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setupExpressions();
        this.onConfigurationUpdated();
    }

    protected destroyExpressions(): void {
        this.textExpression?.dispose();
        this.textExpression = null;
        super.destroyExpressions();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                if (this.textExpressionSource != this.visibleFieldConfiguration.helpMessage) {
                    this.setupExpressions();
                }
            });
    }

    protected setupExpressions(): void {
        if (this.textExpression) {
            this.textExpression.dispose();
            this.textExpression = null;
        }
        if (this.visibleFieldConfiguration.helpMessage) {
            this.textExpressionSource = this.visibleFieldConfiguration.helpMessage;
            this.textExpression = new TextWithExpressions(
                this.visibleFieldConfiguration.helpMessage,
                this.expressionDependencies,
                `${this.key} description wrapper for help message`,
                this.fieldInstance.getFixedExpressionArguments(),
                this.fieldInstance.getObservableExpressionArguments(),
                this.fieldInstance.scope);
            this.textExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => {
                    this.helpMessage = text;
                });
            this.textExpression.triggerEvaluation();
        } else {
            this.textExpressionSource = null;
            this.helpMessage = null;
        }
    }
}
