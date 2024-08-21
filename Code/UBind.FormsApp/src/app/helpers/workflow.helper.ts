import { Injectable } from "@angular/core";
import { Expression } from "@app/expressions/expression";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { Article, ArticleElement, WorkflowStep } from "@app/models/configuration/workflow-step";
import { Errors } from "@app/models/errors";
import { SectionDisplayMode } from "@app/models/section-display-mode.enum";
import { WorkflowDestination } from "@app/models/workflow-destination";
import { ConfigService } from "@app/services/config.service";
import { StringHelper } from "./string.helper";

/**
 * Helps with workflow logic, including determining the starting step
 */
@Injectable({
    providedIn: 'root',
})
export class WorkflowHelper {

    public constructor(
        private configService: ConfigService,
        private expressionDependencies: ExpressionDependencies,
    ) { }

    public getStartingStepName(): string {
        for (let step in this.configService.workflowSteps) {
            if (this.isStartScreen(step)) {
                return step;
            }
        }

        throw Errors.Product.Configuration(
            "No start screen or start screen expression ended up being true for any of the pages. "
            + " Please review your workflow settings and expressions to ensure at least one start screen "
            + " evaluates to true");
    }

    public getStartingDestination(): WorkflowDestination {
        let startingStepName: string = this.getStartingStepName();
        const workflowStep: WorkflowStep = this.configService.workflowSteps[startingStepName];
        let destination: WorkflowDestination = {
            stepName: startingStepName,
        };
        if (workflowStep.displayMode == SectionDisplayMode.Article
            || workflowStep.displayMode == SectionDisplayMode.ArticleElement
        ) {
            destination.articleIndex = this.getFirstNonHiddenArticleIndex(workflowStep);
            if (destination.articleIndex == -1) {
                throw Errors.Workflow.StartingDestinationHidden(startingStepName);
            }
        }
        if (workflowStep.displayMode == SectionDisplayMode.ArticleElement) {
            const article: Article = workflowStep.articles[destination.articleIndex];
            destination.articleElementIndex = this.getFirstNonHiddenArticleElementIndex(article);
        }
        return destination;
    }

    /**
     * @returns the first non-hidden article index, or -1 if all articles are hidden
     */
    private getFirstNonHiddenArticleIndex(workflowStep: WorkflowStep): number {
        let article: Article;
        let articleIndex: number = -1;
        for (let i: number = 0; i < workflowStep.articles.length; i++) {
            article = workflowStep.articles[i];
            let hidden: boolean = false;
            if (!StringHelper.isNullOrEmpty(article.hiddenExpression)) {
                let hiddenExpression: Expression = new Expression(
                    article.hiddenExpression, this.expressionDependencies, article.heading + ' hidden');
                hidden = hiddenExpression.evaluateAndDispose();
            }
            if (!hidden) {
                // check that at least one article element is visible
                if (this.getFirstNonHiddenArticleElementIndex(article) != -1) {
                    articleIndex = i;
                    break;
                }
            }
        }

        return articleIndex;
    }

    /**
     * @returns the first non hidden article element index for the given article,
     * or -1 if all article elements are hidden.
     */
    private getFirstNonHiddenArticleElementIndex(article: Article): number {
        let articleElementIndex: number = -1;
        for (let i: number = 0; i < article.elements?.length; i++) {
            const articleElement: ArticleElement = article.elements[i];
            let hidden: boolean = false;
            if (!StringHelper.isNullOrEmpty(articleElement.hiddenExpression)) {
                let hiddenExpression: Expression = new Expression(
                    articleElement.hiddenExpression,
                    this.expressionDependencies,
                    article.heading + ` element ${i} hidden`);
                hidden = hiddenExpression.evaluateAndDispose();
            }
            if (!hidden) {
                articleElementIndex = i;
                break;
            }
        }

        return articleElementIndex;
    }

    /**
     * Expressions should have been evaluate before calling this function.
     * @param step
     */
    private isStartScreen(step: string): boolean {
        return this.configService.workflowSteps[step].startScreen;
    }
}
