import {
    AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output, QueryList, ViewChildren,
} from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { FormService } from '@app/services/form.service';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { filter, takeUntil } from 'rxjs/operators';
import { Expression } from '@app/expressions/expression';
import { RenderMethod } from '@app/models/render-method.enum';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { Validatable } from '@app/models/validatable';
import { ChildrenValidityTracker } from '../children-validity-tracker';
import { Widget } from '../widget';
import { ApplicationService } from '@app/services/application.service';
import { SectionDisplayMode as SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { WorkflowStep } from '@app/models/configuration/workflow-step';
import { Article as ArticleConfig } from '@app/models/configuration/workflow-step';
import { SectionWidgetStatus } from './section-widget-status';
import * as _ from 'lodash-es';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { NavigationDirection } from '@app/models/navigation-direction.enum';
import { ArrayHelper } from '@app/helpers/array.helper';
import { EventService } from '@app/services/event.service';
import { TransitionState } from '@app/models/transition-state.enum';
import { ArticleWidget } from '../article/article.widget';

/**
 * An article which is a part of a section.
 */
export interface Article {
    definition: ArticleConfig;
    render: boolean;
    name: string;
    heading: string;
    hiddenExpression: Expression;

    /**
     * The name of the step which this article was created for
     */
    stepName: string;
    index: number;
    sectionArticleElementStartingIndex: number;
    sectionArticleElementEndingIndex: number;
    displayMode: SectionDisplayMode;
    canDisplay: boolean;

    /**
     * We need to know which article elements are renderable in order to
     * calculate the next renderable index, so that we know when there
     * are any more to render. Therefore we keep their renderable
     * statuses here for checking over.
     */
    articleElementRenderStatuses: Array<ArticleElementRenderStatus>;
}

/**
 * We need to know which article elements are renderable in order to
 * calculate the next renderable index, so that we know when there
 * are any more to render.
 */
export interface ArticleElementRenderStatus {
    render: boolean;
    sectionArticleElementIndex: number;
    hiddenExpression: Expression;
}

/**
 * Export section widget component class
 * A section represents a step or a page of the form.
 */
@Component({
    selector: 'section-widget',
    templateUrl: './section.widget.html',
    animations: [
        trigger('sectionSlide', [
            state(TransitionState.Left, style({ transform: 'translateX(-25px)' })),
            state(TransitionState.None, style({ transform: 'translateX(0)' })),
            state(TransitionState.Right, style({ transform: 'translateX(25px)' })),
            transition(`* => ${TransitionState.None}`, animate('500ms 500ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('300ms 100ms ease-out')),
        ]),
        trigger('sectionFade', [
            state(TransitionState.Left, style({ opacity: 0 })),
            state(TransitionState.None, style({ opacity: 100 })),
            state(TransitionState.Right, style({ opacity: 0 })),
            transition(`* => ${TransitionState.None}`, animate('500ms 500ms ease-out')),
            transition(`${TransitionState.None} => *`, animate('300ms 100ms ease-out')),
        ]),
    ],
})

export class SectionWidget extends Widget
    implements Validatable, OnDestroy, OnInit, AfterViewInit, SectionWidgetStatus {

    @Input('stepName')
    public stepName: string;

    @Output()
    public articleRendered: EventEmitter<boolean> = new EventEmitter<boolean>();

    @ViewChildren(ArticleWidget)
    protected articleWidgets: QueryList<ArticleWidget>;

    protected transitionDelayBeforeMs: number = 0;
    protected transitionDelayBetweenMs: number = 0;

    public renderMethod: RenderMethod = RenderMethod.Lazy;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public RenderMethod: typeof RenderMethod = RenderMethod;

    public articles: Array<Article> = new Array<Article>();
    public debug: boolean = false;
    public childrenValidityTracker: ChildrenValidityTracker;
    public displayMode: SectionDisplayMode;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public SectionDisplayMode: typeof SectionDisplayMode = SectionDisplayMode;

    public constructor(
        protected calculationService: CalculationService,
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
        private eventService: EventService,
    ) {
        super();
    }

    public get valid(): boolean {
        return this.childrenValidityTracker.valid;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.generateArticles();
        if (this.displayMode == SectionDisplayMode.Article
            || this.displayMode == SectionDisplayMode.ArticleElement
        ) {
            this.listenToArticleIndexChanges();
        }
        if (this.displayMode == SectionDisplayMode.ArticleElement) {
            this.listenToArticleElementIndexChanges();
        }
        this.childrenValidityTracker = new ChildrenValidityTracker();

        if (this.displayMode == SectionDisplayMode.Article) {
            this.setInitialArticleIndex();
        } else if (this.displayMode == SectionDisplayMode.ArticleElement) {
            this.setInitialArticleElementIndex();
        }
        this.onConfigurationUpdated();
    }

    public ngAfterViewInit(): void {
        this.eventService.sectionWidgetCompletedViewInitSubject.next();
        this.articleWidgets.changes
            .subscribe((q: QueryList<ArticleWidget>) => this.articleRendered.emit(true));
        this.articleWidgets.notifyOnChanges();
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.generateArticles();
            });
    }

    private setInitialArticleIndex(): void {
        let workflowDestination: WorkflowDestination = _.clone(this.applicationService.currentWorkflowDestination);
        if (workflowDestination.articleIndex === undefined || workflowDestination.articleIndex < 0) {
            if (!this.applicationService.lastNavigationDirection
                || this.applicationService.lastNavigationDirection == NavigationDirection.Forward
            ) {
                workflowDestination.articleIndex = this.getFirstArticleIndex();
            } else if (this.applicationService.lastNavigationDirection == NavigationDirection.Backward) {
                workflowDestination.articleIndex = this.getLastArticleIndex();
            }
            this.applicationService.currentWorkflowDestination = workflowDestination;
        }
    }

    private setInitialArticleElementIndex(): void {
        let workflowDestination: WorkflowDestination = _.clone(this.applicationService.currentWorkflowDestination);
        let articleElementIndex: number = workflowDestination.articleElementIndex;
        if (articleElementIndex === undefined || articleElementIndex < 0) {
            if (!this.applicationService.lastNavigationDirection
                || this.applicationService.lastNavigationDirection == NavigationDirection.Forward
            ) {
                articleElementIndex = this.getFirstArticleElementIndex();
            } else if (this.applicationService.lastNavigationDirection == NavigationDirection.Backward) {
                articleElementIndex = this.getLastArticleElementIndex();
            }
        } else if (articleElementIndex < this.getFirstArticleElementIndex()) {
            articleElementIndex = this.getFirstArticleElementIndex();
        }
        let articleIndex: number = this.getArticleIndexForArticleElementIndex(articleElementIndex);
        if (articleElementIndex != -1 && articleIndex != -1) {
            workflowDestination.articleIndex = articleIndex;
            workflowDestination.articleElementIndex = articleElementIndex;
            this.applicationService.currentWorkflowDestination = workflowDestination;
        }
    }

    public getArticleIndexForArticleElementIndex(articleElementIndex: number): number {
        for (let article of this.articles) {
            if (articleElementIndex >= article.sectionArticleElementStartingIndex
                && articleElementIndex <= article.sectionArticleElementEndingIndex
            ) {
                return article.index;
            }
        }
        return -1;
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    private generateArticles(): void {
        let articles: Array<Article> = new Array<Article>();
        if (this.configService.workflowSteps[this.stepName]) {
            let stepConfig: WorkflowStep = this.configService.workflowSteps[this.stepName];
            this.displayMode = this.evaluateDisplayMode(stepConfig, this.stepName);
            let sourceItems: Array<ArticleConfig> = stepConfig.articles;
            if (sourceItems) {
                let sectionArticleElementStartingIndex: number = 0;
                for (let index: number = 0; index < sourceItems.length; index++) {
                    let articleConfig: ArticleConfig = sourceItems[index];
                    let sectionArticleElementEndingIndex: number = articleConfig.elements
                        ? sectionArticleElementStartingIndex + articleConfig.elements.length - 1
                        : sectionArticleElementStartingIndex;
                    let article: Article = <Article>{
                        definition: articleConfig,
                        render: true,
                        name: articleConfig.name || articleConfig.heading,
                        heading: articleConfig.heading,
                        hiddenExpression: null,
                        stepName: this.stepName,
                        index: index,
                        sectionArticleElementStartingIndex: sectionArticleElementStartingIndex,
                        sectionArticleElementEndingIndex: sectionArticleElementEndingIndex,
                        displayMode: this.displayMode,
                        canDisplay: false,
                    };
                    if (this.displayMode == SectionDisplayMode.ArticleElement) {
                        this.generateArticleElementRenderStatuses(article, articleConfig);
                    }
                    article.canDisplay = this.canArticleBeDisplayed(article);
                    let hiddenExpressionSource: any = articleConfig.hiddenExpression
                        ? articleConfig.hiddenExpression
                        : (<any>articleConfig).hidden;
                    if (hiddenExpressionSource) {
                        article.render = false;
                        let hiddenExpression: Expression = new Expression(hiddenExpressionSource,
                            this.expressionDependencies, article.heading + ' hidden');
                        hiddenExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                            .subscribe((result: boolean) => {
                                if (article.render != !result) {
                                    article.render = !result;
                                    this.eventService.articleVisibleChangeSubject.next();
                                }
                            });
                        hiddenExpression.triggerEvaluation();
                        article.hiddenExpression = hiddenExpression;
                    }
                    articles.push(article);
                    if (articleConfig.elements) {
                        sectionArticleElementStartingIndex += articleConfig.elements.length;
                    }
                }
            }
        }
        ArrayHelper.synchronise<Article>(
            articles,
            this.articles,
            (item1: Article, item2: Article): boolean => {
                return _.isEqual(item1.definition, item2.definition);
            },
            (item: Article): void => {
                if (item.hiddenExpression) {
                    item.hiddenExpression.dispose();
                }
            });
    }

    private evaluateDisplayMode(stepConfig: WorkflowStep, stepName: string): SectionDisplayMode {
        let displayMode: SectionDisplayMode = stepConfig.displayMode || SectionDisplayMode.Page;
        if (stepConfig.displayModeExpression) {
            displayMode = new Expression(
                stepConfig.displayModeExpression,
                this.expressionDependencies,
                `workflow step ${stepName} display mode expression`).evaluateAndDispose();
        }
        return displayMode;
    }

    private generateArticleElementRenderStatuses(article: Article, articleConfig: ArticleConfig): void {
        article.articleElementRenderStatuses = new Array<ArticleElementRenderStatus>();
        let sectionArticleElementIndex: number = article.sectionArticleElementStartingIndex;
        for (let articleElementConfig of articleConfig.elements) {
            let articleElementRenderStatus: ArticleElementRenderStatus = {
                render: true,
                sectionArticleElementIndex: sectionArticleElementIndex,
                hiddenExpression: null,
            };
            let hiddenExpressionSource: string = articleElementConfig.hiddenExpression
                || (<any>articleElementConfig).hidden;
            if (hiddenExpressionSource) {
                articleElementRenderStatus.render = false;
                articleElementRenderStatus.hiddenExpression = new Expression(
                    hiddenExpressionSource,
                    this.expressionDependencies,
                    articleConfig.heading + ' element hidden');
                articleElementRenderStatus.hiddenExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((result: boolean) => articleElementRenderStatus.render = !result);
                articleElementRenderStatus.hiddenExpression.triggerEvaluation();
            }
            article.articleElementRenderStatuses.push(articleElementRenderStatus);
            sectionArticleElementIndex++;
        }
    }

    public isFirstArticle(): boolean {
        return this.getPreviousArticleIndex() == -1;
    }

    public isLastArticle(): boolean {
        return this.getNextArticleIndex() == -1;
    }

    public getFirstArticleIndex(): number {
        for (let article of this.articles) {
            if (article.render) {
                return article.index;
            }
        }

        return -1;
    }

    public getLastArticleIndex(): number {
        let articlesReversed: Array<Article> = _.clone(this.articles).reverse();
        for (let article of articlesReversed) {
            if (article.render) {
                return article.index;
            }
        }
        return -1;
    }

    public hasNextArticle(): boolean {
        return !this.isLastArticle();
    }

    public hasPreviousArticle(): boolean {
        return !this.isFirstArticle();
    }

    public getCurrentArticleIndex(): number {
        return this.applicationService.currentWorkflowDestination.articleIndex;
    }

    public getNextArticleIndex(): number {
        return this.getSubsequentArticleIndex(this.articles, this.getCurrentArticleIndex());
    }

    public getPreviousArticleIndex(): number {
        let articlesReversed: Array<Article> = _.clone(this.articles).reverse();
        return this.getSubsequentArticleIndex(articlesReversed, this.getCurrentArticleIndex());
    }

    private getSubsequentArticleIndex(articles: Array<Article>, index: number): number {
        let matched: boolean = false;
        for (let article of articles) {
            if (matched && article.render) {
                return article.index;
            }
            if (article.index == index) {
                matched = true;
            }
        }
        return -1;
    }

    public isFirstArticleElement(): boolean {
        return this.getPreviousArticleElementIndex() == -1;
    }

    public isLastArticleElement(): boolean {
        return this.getNextArticleElementIndex() == -1;
    }

    public getFirstArticleElementIndex(): number {
        for (let article of this.articles) {
            if (article.render) {
                for (let articleElementRenderStatus of article.articleElementRenderStatuses) {
                    if (articleElementRenderStatus.render) {
                        return articleElementRenderStatus.sectionArticleElementIndex;
                    }
                }
            }
        }
        return -1;
    }

    public getLastArticleElementIndex(): number {
        let articlesReversed: Array<Article> = _.clone(this.articles).reverse();
        for (let article of articlesReversed) {
            if (article.render) {
                let articleElementRenderStatusesReversed: Array<ArticleElementRenderStatus> =
                    _.clone(article.articleElementRenderStatuses).reverse();
                for (let articleElementRenderStatus of articleElementRenderStatusesReversed) {
                    if (articleElementRenderStatus.render) {
                        return articleElementRenderStatus.sectionArticleElementIndex;
                    }
                }
            }
        }
        return -1;
    }

    public hasNextArticleElement(): boolean {
        return !this.isLastArticleElement();
    }

    public hasPreviousArticleElement(): boolean {
        return !this.isFirstArticleElement();
    }

    public getCurrentArticleElementIndex(): number {
        return this.applicationService.currentWorkflowDestination.articleElementIndex;
    }

    public getNextArticleElementIndex(): number {
        let currentArticleElementIndex: number = this.getCurrentArticleElementIndex();
        if (currentArticleElementIndex < 0) {
            return -1;
        }
        let targetArticleElementIndex: number = currentArticleElementIndex + 1;
        let articleIndex: number = 0;
        while (articleIndex < this.articles.length) {
            let article: Article = this.articles[articleIndex];
            if (targetArticleElementIndex >= article.sectionArticleElementStartingIndex
                && targetArticleElementIndex <= article.sectionArticleElementEndingIndex
            ) {
                let articleElementIndex: number = article.sectionArticleElementStartingIndex;
                for (let articleElementRenderStatus of article.articleElementRenderStatuses) {
                    if (targetArticleElementIndex == articleElementIndex) {
                        if (articleElementRenderStatus.render) {
                            return targetArticleElementIndex;
                        } else {
                            targetArticleElementIndex++;
                        }
                    }
                    articleElementIndex++;
                }
            }
            articleIndex++;
        }

        return -1;
    }

    public getPreviousArticleElementIndex(): number {
        let currentArticleElementIndex: number = this.getCurrentArticleElementIndex();
        let targetArticleElementIndex: number = currentArticleElementIndex - 1;
        let articleIndex: number = this.articles.length - 1;
        while (articleIndex >= 0) {
            let article: Article = this.articles[articleIndex];
            if (targetArticleElementIndex >= article.sectionArticleElementStartingIndex
                && targetArticleElementIndex <= article.sectionArticleElementEndingIndex
            ) {
                let articleElementIndex: number = article.sectionArticleElementEndingIndex;
                let articleElementRenderStatusesReversed: Array<ArticleElementRenderStatus> =
                    _.clone(article.articleElementRenderStatuses).reverse();
                for (let articleElementRenderStatus of articleElementRenderStatusesReversed) {
                    if (targetArticleElementIndex == articleElementIndex) {
                        if (articleElementRenderStatus.render) {
                            return targetArticleElementIndex;
                        } else {
                            targetArticleElementIndex--;
                        }
                    }
                    articleElementIndex--;
                }
            }
            articleIndex--;
        }

        return -1;
    }

    public getCurrentWorkflowStepDisplayMode(): SectionDisplayMode {
        return this.displayMode;
    }

    private listenToArticleIndexChanges(): void {
        const thisArg: any = this;
        this.applicationService.articleIndexSubject.pipe(
            takeUntil(this.destroyed),
            filter(() => this.stepName == this.applicationService.currentWorkflowDestination.stepName),
        ).subscribe((articleIndex: number) => {
            // We need the setTimeout to trigger change detection here.
            setTimeout(() => {
                for (let article of thisArg.articles) {
                    article.canDisplay = article.index == articleIndex;
                }
            }, 0);
        });
    }

    private listenToArticleElementIndexChanges(): void {
        const thisArg: any = this;
        this.applicationService.articleIndexSubject.pipe(
            takeUntil(this.destroyed),
            filter(() => this.stepName == this.applicationService.currentWorkflowDestination.stepName),
        ).subscribe((articleElementIndex: number) => {
            for (let article of thisArg.articles) {
                article.canDisplay =
                    articleElementIndex >= article.sectionArticleElementStartingIndex
                    && articleElementIndex <= article.sectionArticleElementEndingIndex;
            }
        });
    }

    private canArticleBeDisplayed(article: Article): boolean {
        switch (this.displayMode) {
            case SectionDisplayMode.Article:
                return this.applicationService.currentWorkflowDestination.articleIndex == article.index;
            case SectionDisplayMode.ArticleElement: {
                const articleElementIndex: number
                    = this.applicationService.currentWorkflowDestination.articleElementIndex;
                return articleElementIndex >= article.sectionArticleElementStartingIndex
                    && articleElementIndex <= article.sectionArticleElementEndingIndex;
            }
            case SectionDisplayMode.Page:
                return true;
            default:
                throw new Error(`Unknown display mode ${this.displayMode}. `
                    + `Please ensure you set the correct display mode for the workflow step.`);
        }
    }
}
