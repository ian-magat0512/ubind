import {
    Component, ViewChild, ViewContainerRef, OnInit } from '@angular/core';
import { Wrapper } from '../wrapper';
import { takeUntil } from 'rxjs/operators';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { EventService } from '@app/services/event.service';

// eslint-disable-next-line no-unused-vars
declare let $: any; // please don't tamper with this line

/**
 * Export tooltip wrapper component class.
 * This class contains configuration of the Tooltip.
 * It is used to handle tooltip hide and show and flipping functionality. 
 */
@Component({
    selector: 'tooltip-wrapper',
    templateUrl: './tooltip.wrapper.html',
})

export class TooltipWrapper extends Wrapper implements OnInit {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    private contentExpression: TextWithExpressions;
    public tooltipContent: string;
    private visibleFieldConfiguration: VisibleFieldConfiguration;
    public constructor(
        private expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setupContentExpression();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.visibleFieldConfiguration = <VisibleFieldConfiguration>configs.new;
                this.setupContentExpression();
            });
    }

    protected destroyExpressions(): void {
        this.contentExpression?.dispose();
        this.contentExpression = null;
    }

    protected setupContentExpression(): void {
        if (this.contentExpression) {
            this.contentExpression.dispose();
            this.contentExpression = null;
        }
        if (this.visibleFieldConfiguration.tooltip) {
            this.contentExpression = new TextWithExpressions(
                this.visibleFieldConfiguration.tooltip,
                this.expressionDependencies,
                `${this.fieldKey} tooltip text`,
                this.fieldInstance.getFixedExpressionArguments(),
                this.fieldInstance.getObservableExpressionArguments(),
                this.fieldInstance.scope);
            this.contentExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((content: string) => this.tooltipContent = content);
            this.contentExpression.triggerEvaluation();
        } else {
            this.tooltipContent = null;
        }
    }
}
