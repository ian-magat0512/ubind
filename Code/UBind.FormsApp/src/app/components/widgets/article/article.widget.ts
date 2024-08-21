import { Component, Input, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { DynamicWidget } from '../dynamic.widget';
import { Expression } from '@app/expressions/expression';
import { FormService } from '@app/services/form.service';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { SubscriptionLike } from 'rxjs';
import { distinctUntilChanged, filter, takeUntil } from 'rxjs/operators';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { RenderMethod } from '@app/models/render-method.enum';
import { Validatable } from '@app/models/validatable';
import { ChildrenValidityTracker } from '../children-validity-tracker';
import { QuestionsWidget } from '../questions/questions.widget';
import { ApplicationService } from '@app/services/application.service';
import { Hideable } from '@app/models/hideable';
import { Article as ArticleConfig } from '@app/models/configuration/workflow-step';
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import * as _ from 'lodash-es';
import { ArticleElement } from './article-element';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { ArrayHelper } from '@app/helpers/array.helper';
import { EventService } from '@app/services/event.service';
import { TextElementsForStep } from '@app/models/configuration/working-configuration';
import { StringHelper } from '@app/helpers/string.helper';
import { TransitionState } from '@app/models/transition-state.enum';
import { FieldGroup } from '@app/models/configuration/field-group';
import { Field } from '@app/models/configuration/field';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';

/**
 * Export article widget component class.
 * This class manage article widgets functions.
 */
@Component({
    selector: 'article-widget',
    templateUrl: './article.widget.html',
    animations: [
        trigger('articleSlide', [
            state(TransitionState.Left, style({ transform: 'translateY(-10%)' })),
            state(TransitionState.None, style({ transform: 'translateY(0)' })),
            state(TransitionState.Right, style({ transform: 'translateY(-10%)' })),
            transition(`* => ${TransitionState.None}`, animate('400ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('200ms 1100ms ease-out')),
        ]),
        trigger('articleFade', [
            state(TransitionState.Left, style({ opacity: 0 })),
            state(TransitionState.None, style({ opacity: 100 })),
            state(TransitionState.Right, style({ opacity: 0 })),
            transition(`* => ${TransitionState.None}`, animate('400ms ease-in-out')),
            transition(`${TransitionState.None} => *`, animate('200ms 100ms ease-in-out')),
        ]),
    ],
})
export class ArticleWidget extends DynamicWidget implements OnInit, OnDestroy, Validatable, Hideable {

    @ViewChildren(QuestionsWidget)
    protected questionsWidgets: QueryList<QuestionsWidget>;

    @Input('definition')
    protected articleDefinition: ArticleConfig;

    /**
     * The name of the step/page this article belongs to.
     */
    @Input('stepName')
    public stepName: string;

    /**
     * The index of the article within the section.
     */
    @Input('index')
    public index: number;

    /**
     * Whether to display a single article element or article at a time
     */
    @Input('displayMode')
    public displayMode: SectionDisplayMode;

    /**
     * The starting index of the first article element within the section/workflow step
     */
    @Input('sectionArticleElementStartingIndex')
    public sectionArticleElementStartingIndex: number;

    /**
     * A reference to the section widget that this article is a part of.
     */
    @Input('parentSectionWidgetChildrenValidityTracker')
    protected parentSectionWidgetChildrenValidityTracker: ChildrenValidityTracker;

    /**
     * A reference to the hiddenExpression for debugging purposes.
     */
    @Input('hiddenExpression')
    public hiddenExpression: Expression;

    public articleElements: Array<ArticleElement> = new Array<ArticleElement>();

    public name: string;
    public heading: string;
    public text: string;

    public cssClass: string = '';
    protected headingExpressionSource: string;
    protected textExpressionSource: string;

    protected headingExpression: TextWithExpressions;
    protected textExpression: TextWithExpressions;

    protected headingExpressionSubscription: SubscriptionLike;
    protected textExpressionSubscription: SubscriptionLike;

    protected transitionDelayBeforeMs: number = 400;
    protected transitionDelayBetweenMs: number = 300;

    public renderMethod: RenderMethod = RenderMethod.Lazy;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public RenderMethod: typeof RenderMethod = RenderMethod;
    public childrenValidityTracker: ChildrenValidityTracker;

    private hidden: boolean = false;

    public constructor(
        protected calculationService: CalculationService,
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
        protected workflowDestinationService: WorkflowDestinationService,
        private workflowStatusService: WorkflowStatusService,
        private eventService: EventService,
        private unifiedFormModelService: UnifiedFormModelService,
    ) {
        super(workflowService, configService, workflowDestinationService);
    }

    public get valid(): boolean {
        return this.childrenValidityTracker.valid;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.name = this.articleDefinition.name || this.articleDefinition.heading;
        this.generateElements();
        if (this.displayMode == SectionDisplayMode.ArticleElement) {
            this.listenToArticleElementIndexChanges();
        }
        this.setupExpressions();
        this.childrenValidityTracker = new ChildrenValidityTracker();
        this.handleValidityChanges();
        this.handleBeingHidden();
        if (this.displayMode == SectionDisplayMode.ArticleElement) {
            this.setArticleElementsToDisplay(this.applicationService.currentWorkflowDestination.articleElementIndex);
        }
        this.initializeCssClass();
        this.onConfigurationUpdated();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.generateElements();
                this.setupExpressions();
                this.initializeCssClass();
            });
    }

    public ngOnDestroy(): void {
        this.questionsWidgets = undefined;
        this.headingExpressionSubscription?.unsubscribe();
        this.textExpressionSubscription?.unsubscribe();
        super.ngOnDestroy();
    }

    protected destroyExpressions(): void {
        this.headingExpression?.dispose();
        this.headingExpression = null;
        this.textExpression?.dispose();
        this.textExpression = null;
        super.destroyExpressions();
    }

    protected async onChangeStepBeforeAnimateIn(
        // eslint-disable-next-line no-unused-vars
        previousDestination: WorkflowDestination,
        // eslint-disable-next-line no-unused-vars
        nextDestination: WorkflowDestination,
    ): Promise<void> {
        // load the new heading & text etc for this step
        this.setupExpressions();
    }

    private getTextElementsForStep(): TextElementsForStep {
        return this.configService.textElements?.workflow?.[this.workflowService.currentNavigation.to.stepName];
    }

    protected setupExpressions(): void {
        const textCurrentStep: TextElementsForStep = this.getTextElementsForStep();
        const heading: any = this.articleDefinition.heading;
        const text: string = this.articleDefinition.text;
        const tempHeadExpression: any = (heading && textCurrentStep && textCurrentStep[heading])
            ? textCurrentStep[heading].text
            : heading;
        const tempTextExpression: any = (text && textCurrentStep && textCurrentStep[text])
            ? textCurrentStep[text].text
            : text;
        if (tempHeadExpression != null && tempHeadExpression != this.headingExpressionSource) {
            this.headingExpressionSource = tempHeadExpression;
            this.setupHeadingExpression();
        }
        if (tempTextExpression != null && tempTextExpression != this.textExpressionSource) {
            this.textExpressionSource = tempTextExpression;
            this.setupTextExpression();
        }
    }

    protected setupHeadingExpression(): void {
        if (this.headingExpressionSubscription) {
            this.headingExpressionSubscription.unsubscribe();
            this.headingExpression.dispose();
        }
        if (!StringHelper.isNullOrEmpty(this.headingExpressionSource)) {
            this.headingExpression = new TextWithExpressions(
                this.headingExpressionSource,
                this.expressionDependencies,
                `${this.stepName} article ${this.index} heading text`);
            this.headingExpressionSubscription
                = this.headingExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((value: string) => this.heading = value);
            this.headingExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.heading = null;
        }
    }

    protected setupTextExpression(): void {
        if (this.textExpressionSubscription) {
            this.textExpressionSubscription.unsubscribe();
            this.textExpression.dispose();
        }
        if (!StringHelper.isNullOrEmpty(this.textExpressionSource)) {
            this.textExpression = new TextWithExpressions(
                this.textExpressionSource,
                this.expressionDependencies,
                `${this.stepName} article ${this.index} text`);
            this.textExpressionSubscription = this.textExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.text = value);
            this.textExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.text = null;
        }
    }

    protected generateElements(): void {
        let articleElements: Array<ArticleElement> = new Array<ArticleElement>();
        if (this.articleDefinition.elements) {
            let sourceItems: any = this.articleDefinition.elements;
            let sectionArticleElementIndex: number = this.sectionArticleElementStartingIndex;
            for (let item of sourceItems) {
                let articleElement: ArticleElement = <ArticleElement>{
                    definition: item,
                    name: item.name,
                    key: item.name,
                    type: item.type,
                    affectsPremium: item.affectsPremium,
                    affectsTriggers: item.affectsTriggers,
                    requiredForCalculation: item.requiredForCalculation || item.requiredForCalculations,
                    render: true,
                    hiddenExpression: null,
                    stepName: this.stepName,
                    sectionArticleElementIndex: sectionArticleElementIndex,
                    canDisplay: true,
                };
                let hiddenExpressionSource: string = item.hiddenExpression ? item.hiddenExpression : item.hidden;
                if (hiddenExpressionSource) {
                    articleElement.render = false;
                    articleElement.hiddenExpression = new Expression(
                        hiddenExpressionSource,
                        this.expressionDependencies,
                        this.articleDefinition.heading + ' hidden');
                    articleElement.hiddenExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                        .subscribe((result: boolean) => {
                            if (articleElement.render != !result) {
                                articleElement.render = !result;
                                this.eventService.questionSetVisibleChangeSubject.next();
                            }
                        });
                    articleElement.hiddenExpression.triggerEvaluation();
                }
                articleElements.push(articleElement);
                sectionArticleElementIndex++;
            }
        }
        ArrayHelper.synchronise<ArticleElement>(
            articleElements,
            this.articleElements,
            (item1: ArticleElement, item2: ArticleElement): boolean => _.isEqual(item1.definition, item2.definition),
            (item: ArticleElement): void => {
                if (item.hiddenExpression) {
                    item.hiddenExpression.dispose();
                }
            });
        this.hideQuestionSetsForHiddenArticleElements();
    }

    private initializeCssClass(): void {
        if (this.articleDefinition.cssClasses) {
            let cssClasses: string = "";
            this.articleDefinition.cssClasses.forEach((cssClass: string) => {
                cssClasses += cssClass.trim() + " ";
            });
            this.cssClass = cssClasses.trimEnd();
        } else {
            this.cssClass = this.articleDefinition?.cssClass;
        }
    }

    private handleValidityChanges(): void {
        this.childrenValidityTracker.validObservable
            .pipe(
                takeUntil(this.destroyed),
                distinctUntilChanged(),
            )
            .subscribe((valid: boolean) => {
                this.parentSectionWidgetChildrenValidityTracker.onChildValidityChange(this.stepName, valid);
            });
    }

    private handleBeingHidden(): void {
        if (this.hiddenExpression) {
            this.hiddenExpression.nextResultObservable.pipe(
                takeUntil(this.destroyed),
                // Once we start navigating out, we don't want to respond to hiding situations
                // because the process of navigating out would hide question sets and then
                // cause their data to be wiped unintentionally in the unified form model.
                takeUntil(this.workflowStatusService.navigatingOutStarted),
            ).subscribe((hidden: any) => this.setHidden(hidden));
        }
    }

    private hideQuestionSetsForHiddenArticleElements() {
        const hiddenArticleElements: Array<ArticleElement> = this.articleElements.filter(
            (articleElement: ArticleElement) => !articleElement.render && articleElement.type == 'questions');
        for (let hiddenArticleElement of hiddenArticleElements) {
            if (!hiddenArticleElement.hiddenExpression) {
                continue;
            }
            const hiddenQuestionSets: Array<FieldGroup> =
                this.configService.questionSets[hiddenArticleElement.name];
            if(hiddenQuestionSets !== undefined) {
                hiddenQuestionSets.forEach((hiddenQuestionSet: FieldGroup) =>
                    hiddenQuestionSet.fieldGroup.forEach((f: Field) => {
                        this.unifiedFormModelService.strictFormModel
                            .deleteQuestionSetModelFromUnifiedFormModel(f.key);
                    }));
            }
        }
    }

    /**
     * Sets this article widget as hidden, and also notifies children that
     * this article has been hidden to so fields can notify subscribers that
     * their value should be considered empty.
     */
    public setHidden(hidden: boolean): void {
        if (this.hidden != hidden) {
            this.hidden = hidden;
            this.questionsWidgets.forEach((questionsWidget: QuestionsWidget) => {
                questionsWidget.onParentHiddenChange(hidden);
            });
            this.eventService.formElementHiddenChangeSubject.next();
        }
    }

    public isHidden(): boolean {
        return this.hidden;
    }

    private listenToArticleElementIndexChanges(): void {
        this.applicationService.articleElementIndexSubject.pipe(
            takeUntil(this.destroyed),
            filter(() => this.stepName == this.applicationService.currentWorkflowDestination.stepName),
        ).subscribe((articleElementIndex: number) => {
            this.setArticleElementsToDisplay(articleElementIndex);
        });
    }

    private setArticleElementsToDisplay(articleElementIndex: number): void {
        for (let articleElement of this.articleElements) {
            articleElement.canDisplay = articleElementIndex == articleElement.sectionArticleElementIndex;
        }
    }
}
