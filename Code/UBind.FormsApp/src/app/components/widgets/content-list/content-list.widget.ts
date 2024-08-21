import { Directive, OnInit } from "@angular/core";
import { Expression } from "@app/expressions/expression";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { ArrayHelper } from "@app/helpers/array.helper";
import { ContentDefinition } from "@app/models/configuration/workflow-step";
import { ApplicationService } from "@app/services/application.service";
import { ConfigService } from "@app/services/config.service";
import { WorkflowService } from "@app/services/workflow.service";
import { takeUntil } from "rxjs/operators";
import { Widget } from "../widget";
import * as _ from 'lodash-es';

/**
 * Represents a content element and its current state
 */
interface ContentElementState {
    definition: ContentDefinition;
    name: string;
    textElement?: string;
    content?: string;
    hiddenExpressionSource?: string;
    hiddenExpression: Expression;
    render: boolean;
    cssClass: string;
}

/**
 * A list of content elements.
 */
@Directive()
export abstract class ContentListWidget extends Widget implements OnInit {

    public contentElements: Array<ContentElementState>;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.generateContentElements();
    }

    protected abstract getContentDefinitions(): Array<ContentDefinition>;

    protected generateContentElements(): void {
        let contentDefinitions: Array<ContentDefinition> = this.getContentDefinitions();
        let contentElements: Array<ContentElementState> = new Array<ContentElementState>();
        if (contentDefinitions) {
            for (let contentDefinition of contentDefinitions) {
                let contentElementState: ContentElementState = {
                    definition: contentDefinition,
                    name: contentDefinition.name,
                    textElement: contentDefinition.textElement,
                    content: contentDefinition.content,
                    hiddenExpressionSource: contentDefinition.hiddenExpression,
                    hiddenExpression: null,
                    render: true,
                    cssClass: contentDefinition.cssClass,
                };
                if (contentElementState.hiddenExpressionSource) {
                    contentElementState.render = false;
                    contentElementState.hiddenExpression = new Expression(
                        contentElementState.hiddenExpressionSource,
                        this.expressionDependencies,
                        `content ${contentDefinition.name} hidden`);
                    contentElementState.hiddenExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                        .subscribe((result: boolean) => {
                            if (contentElementState.render != !result) {
                                contentElementState.render = !result;
                            }
                        });
                    contentElementState.hiddenExpression.triggerEvaluation();
                }
                contentElements.push(contentElementState);
            }
        }
        if (!this.contentElements) {
            this.contentElements = contentElements;
        } else {
            ArrayHelper.synchronise<ContentElementState>(
                contentElements,
                this.contentElements,
                (item1: ContentElementState, item2: ContentElementState): boolean => _.isEqual(
                    item1.definition, item2.definition),
                (item: ContentElementState): void => {
                    if (item.hiddenExpression) {
                        item.hiddenExpression.dispose();
                    }
                });
        }
    }

    protected destroyExpressions(): void {
        for (let contentElement of this.contentElements) {
            contentElement.hiddenExpression?.dispose();
            contentElement.hiddenExpression = null;
        }
    }
}
