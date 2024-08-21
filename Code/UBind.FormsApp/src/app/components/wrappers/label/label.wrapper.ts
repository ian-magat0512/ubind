import { Component, ViewChild, ViewContainerRef, OnInit } from '@angular/core';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { EventService } from '@app/services/event.service';
import { takeUntil } from 'rxjs/operators';
import { Wrapper } from '../wrapper';

/**
 * Export label wrapper component class.
 * TODO: Write a better class header: label wrapper functions.
 */
@Component({
    selector: 'label-wrapper',
    templateUrl: './label.wrapper.html',
    styleUrls: ['./label.wrapper.css'],
})

export class LabelWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    public label: string;
    private visibleFieldConfiguration: VisibleFieldConfiguration;
    private labelExpression: TextWithExpressions;

    public constructor(
        protected expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setupLabelExpression();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.visibleFieldConfiguration = <VisibleFieldConfiguration>configs.new;
                this.setupLabelExpression();
            });
    }

    protected destroyExpressions(): void {
        this.labelExpression?.dispose();
        this.labelExpression = null;
        super.destroyExpressions();
    }

    protected setupLabelExpression(): void {
        if (this.labelExpression) {
            this.labelExpression.dispose();
            this.labelExpression = null;
        }
        if (this.visibleFieldConfiguration.label) {
            this.labelExpression = new TextWithExpressions(
                this.visibleFieldConfiguration.label,
                this.expressionDependencies,
                this.fieldKey + ' label',
                this.fieldInstance.getFixedExpressionArguments(),
                this.fieldInstance.getObservableExpressionArguments(),
                this.fieldInstance.scope);
            this.labelExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => this.label = text);
            this.labelExpression.triggerEvaluation();
        } else {
            this.label = null;
        }
    }
}
