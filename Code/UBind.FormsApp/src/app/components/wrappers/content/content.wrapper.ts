import { Component, ViewChild, ViewContainerRef, OnInit, OnDestroy } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { EventService } from '@app/services/event.service';
import { takeUntil } from 'rxjs/operators';
import { Wrapper } from '../wrapper';

/**
 * Export content wrapper component class.
 * TODO: Write a better class header: content wrapper functions.
 */
@Component({
    selector: 'content-wrapper',
    templateUrl: './content.wrapper.html',
})
export class ContentWrapper extends Wrapper implements OnInit, OnDestroy {

    @ViewChild('fieldComponent', { read: ViewContainerRef, static: true }) public fieldComponent: ViewContainerRef;

    public values: any = {};
    private visibleFieldConfiguration: VisibleFieldConfiguration;
    private textExpressionMap: Map<string, TextWithExpressions> = new Map<string, TextWithExpressions>();

    public constructor(
        private expressionDependencies: ExpressionDependencies,
        protected sanitizer: DomSanitizer,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.visibleFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.setupValueExpressions();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.visibleFieldConfiguration = <VisibleFieldConfiguration>configs.new;
                this.setupValueExpressions();
            });
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    protected setupValueExpressions(): void {
        this.setupValueExpression('heading2', this.visibleFieldConfiguration.heading2);
        this.setupValueExpression('heading3', this.visibleFieldConfiguration.heading3);
        this.setupValueExpression('heading4', this.visibleFieldConfiguration.heading4);
        this.setupValueExpression('paragraph', this.visibleFieldConfiguration.paragraph);
        this.setupValueExpression('customHTML', this.visibleFieldConfiguration.html);
        this.setupValueExpression('terms', this.visibleFieldConfiguration.htmlTermsAndConditions);
    }

    protected setupValueExpression(contentItemKey: string, source: string): void {
        let textExpression: TextWithExpressions = this.textExpressionMap.get(contentItemKey);
        if (textExpression) {
            textExpression.dispose();
            this.textExpressionMap.delete(contentItemKey);
        }
        if (source) {
            textExpression = new TextWithExpressions(
                source,
                this.expressionDependencies,
                `${this.key} content wrapper ${contentItemKey}`,
                this.fieldInstance.getFixedExpressionArguments(),
                this.fieldInstance.getObservableExpressionArguments(),
                this.fieldInstance.scope);
            textExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => {
                    this.values[contentItemKey] = text;
                });
            this.textExpressionMap.set(contentItemKey, textExpression);
            textExpression.triggerEvaluation();
        } else {
            this.values[contentItemKey] = null;
        }
    }
}
