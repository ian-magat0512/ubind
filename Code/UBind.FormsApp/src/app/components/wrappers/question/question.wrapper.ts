import { Component, ViewChild, ViewContainerRef, OnInit } from '@angular/core';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { StringHelper } from '@app/helpers/string.helper';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { EventService } from '@app/services/event.service';
import { takeUntil } from 'rxjs/operators';
import { Wrapper } from '../wrapper';

/**
 * Export question wrapper component class.
 * This class manage question wrapper functions.
 */
@Component({
    selector: 'question-wrapper',
    templateUrl: './question.wrapper.html',
})

export class QuestionWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    public question: string;
    private visibleFieldConfiguration: VisibleFieldConfiguration;
    private textExpression: TextWithExpressions;

    public constructor(
        protected expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setupTextExpression();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.visibleFieldConfiguration = <VisibleFieldConfiguration>configs.new;
                this.setupTextExpression();
            });
    }

    protected destroyExpressions(): void {
        this.textExpression?.dispose();
        this.textExpression = null;
        super.destroyExpressions();
    }

    protected setupTextExpression(): void {
        if (this.textExpression) {
            this.textExpression.dispose();
        }
        let textSource: any = this.visibleFieldConfiguration.question;
        if (!StringHelper.isNullOrEmpty(textSource)) {
            this.textExpression = new TextWithExpressions(
                textSource,
                this.expressionDependencies,
                this.fieldKey + ' questionWrapper',
                this.fieldInstance.getFixedExpressionArguments(),
                this.fieldInstance.getObservableExpressionArguments(),
                this.fieldInstance.scope);
            this.textExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => this.question = text);
            this.textExpression.triggerEvaluation();
        } else {
            this.question = null;
        }
    }
}
