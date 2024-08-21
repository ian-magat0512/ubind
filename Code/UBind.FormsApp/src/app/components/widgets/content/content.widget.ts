import { Component, Input, OnInit } from '@angular/core';
import { Widget } from '../widget';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { takeUntil } from 'rxjs/operators';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { EventService } from '@app/services/event.service';
import { TextElementsForStep } from '@app/models/configuration/working-configuration';

/**
 * Export content widget component class.
 * TODO: Write a better class header: content widget functions.
 */
@Component({
    selector: 'content-widget',
    templateUrl: './content.widget.html',
})

export class ContentWidget extends Widget implements OnInit {

    @Input('name')
    private name: string;

    @Input('textElement')
    private textElement: string;

    @Input('content')
    private inlineContent: string;

    public content: string;

    private contentTemplate: string;
    private contentExpression: TextWithExpressions;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.contentTemplate = this.getContentTemplate();
        this.generateContentFromTemplate(this.contentTemplate);
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                let contentTemplate: string = this.getContentTemplate();
                if (contentTemplate != this.contentTemplate) {
                    this.contentTemplate = contentTemplate;
                    this.generateContentFromTemplate(contentTemplate);
                }
            });
    }

    protected destroyExpressions(): void {
        this.contentExpression?.dispose();
        this.contentExpression = null;
        super.destroyExpressions();
    }

    private getContentTemplate(): string {
        let contentTemplate: string = this.inlineContent;
        if (!contentTemplate) {
            let textElementName: string = this.textElement ?? this.name;
            const stepTextElements: TextElementsForStep
                = this.configService.textElements?.workflow?.[this.workflowService.currentDestination.stepName];
            if (stepTextElements) {
                contentTemplate = stepTextElements[textElementName]?.text;
            }
            if (!contentTemplate) {
                contentTemplate = this.configService.textElements?.content?.[textElementName]?.text;
            }
            if (!contentTemplate) {
                contentTemplate = this.name;
            }
        }
        return contentTemplate;
    }

    private generateContentFromTemplate(contentTemplate: string): void {
        if (this.contentExpression) {
            this.contentExpression.dispose();
            this.contentExpression = null;
        }
        if (!contentTemplate) {
            this.content = '';
        } else {
            this.contentExpression = new TextWithExpressions(
                contentTemplate,
                this.expressionDependencies,
                'content widget template text');
            this.contentExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.content = value);
            this.contentExpression.triggerEvaluationWhenFormLoaded();
        }
    }
}
