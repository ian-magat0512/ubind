import { Injectable } from "@angular/core";
import { Expression } from "@app/expressions/expression";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { WorkflowAction } from "@app/models/configuration/workflow-action";
import { FormType } from "@app/models/form-type.enum";
import { NavigationDirection } from "@app/models/navigation-direction.enum";
import { WorkflowDestination } from "@app/models/workflow-destination";
import { ApplicationService } from "./application.service";
import { ConfigService } from "./config.service";

/**
 * Service for calculating/evaluating workflow destinations.
 * This is used by WorkflowService to generate a WorkflowDestination
 */
@Injectable({
    providedIn: 'root',
})
export class WorkflowDestinationService {

    public constructor(
        private applicationService: ApplicationService,
        private expressionDependencies: ExpressionDependencies,
        private configService: ConfigService,
    ) {
    }

    public getDestination(action: WorkflowAction, widgetPosition: string): WorkflowDestination {
        let destination: WorkflowDestination = {
            articleElementIndex: this.getDestinationArticleElementIndex(action, widgetPosition),
            articleIndex: this.getDestinationArticleIndex(action, widgetPosition),
            stepName: this.getDestinationStep(action, widgetPosition),
        };
        return destination;
    }

    private getDestinationStep(action: WorkflowAction, widgetPosition: string): string {
        let destinationStepExpression: string = this.getApplicableProperty(
            action, 'destinationStepExpression', widgetPosition);
        let destinationStep: string;
        if (destinationStepExpression) {
            destinationStep = new Expression(
                destinationStepExpression,
                this.expressionDependencies,
                "workflow destination step").evaluateAndDispose();
        } else {
            destinationStep = this.getApplicableProperty(action, 'destinationStep', widgetPosition);
        }

        return destinationStep;
    }

    private getDestinationArticleElementIndex(action: WorkflowAction, widgetPosition: string): number {
        let destinationArticleElementIndexExpression: string = this.getApplicableProperty(
            action, 'destinationArticleElementIndexExpression', widgetPosition);
        let destinationArticleElementIndex: number;
        if (destinationArticleElementIndexExpression) {
            destinationArticleElementIndex = new Expression(
                destinationArticleElementIndexExpression,
                this.expressionDependencies,
                "workflow destination article element index").evaluateAndDispose();
        } else {
            destinationArticleElementIndex = this.getApplicableProperty(
                action, 'destinationArticleElementIndex', widgetPosition);
        }
        return destinationArticleElementIndex;
    }

    private getDestinationArticleIndex(action: WorkflowAction, widgetPosition: string): number {
        let destinationArticleIndexExpression: string = this.getApplicableProperty(
            action, 'destinationArticleIndexExpression', widgetPosition);
        let destinationArticleIndex: number;
        if (destinationArticleIndexExpression) {
            destinationArticleIndex = new Expression(
                destinationArticleIndexExpression,
                this.expressionDependencies,
                "workflow destination article index").evaluateAndDispose();
        } else {
            destinationArticleIndex = this.getApplicableProperty(
                action, 'destinationArticleIndex', widgetPosition);
        }
        return destinationArticleIndex;
    }

    /**
     * Gets a particular property from the config for a workflow action, with variants by quote state and claim state
     * @param properties 
     * @param propertyName 
     * @param widgetPosition depreacted, but it seems to allow us to grab certain properties defined by widget position.
     * @returns 
     */
    public getApplicableProperty(properties: WorkflowAction, propertyName: string, widgetPosition?: any): any {
        if (!properties) {
            throw new Error("call to getApplicableProperty passed in null properties.");
        }
        if (widgetPosition && properties[widgetPosition] != null) {
            let property: any = this.getApplicableProperty(properties[widgetPosition], propertyName);
            if (property != null) {
                return property;
            }
        }
        let property: any = properties[propertyName];
        if (Object.prototype.toString.call(property) == '[object Object]') {
            for (let key in property) {
                if (this.applicationService.formType == FormType.Quote
                    && key == this.applicationService.latestQuoteResult.quoteState
                ) {
                    return property[key];
                } else if (this.applicationService.formType == FormType.Claim
                    && key == this.applicationService.latestClaimResult.claimState
                ) {
                    return property[key];
                }
            }
            if (property['default'] != null) {
                return property['default'];
            } else {
                return null;
            }
        }
        return property;
    }

    public getNavigationDirection(from: WorkflowDestination, to: WorkflowDestination): NavigationDirection {
        if (!from) {
            return NavigationDirection.Forward;
        }
        const workflowSteps: any = this.configService.workflowSteps;
        const fromStepIndex: number = workflowSteps && workflowSteps[from.stepName]?.tabIndex
            ? workflowSteps[from.stepName].tabIndex
            : 0;
        const toStepIndex: number = workflowSteps && workflowSteps[to.stepName]?.tabIndex
            ? workflowSteps[to.stepName].tabIndex
            : 0;
        const fromDestinationValue: number =
            fromStepIndex * 1000 * 1000 * 1000
            + Math.max(0, from.articleIndex || 0) * 1000 * 1000
            + Math.max(0, from.articleElementIndex || 0) * 1000
            + Math.max(0, from.repeatingInstanceIndex || 0);
        const toDestinationValue: number =
            toStepIndex * 1000 * 1000 * 1000
            + Math.max(0, to.articleIndex || 0) * 1000 * 1000
            + Math.max(0, to.articleElementIndex || 0) * 1000
            + Math.max(0, to.repeatingInstanceIndex || 0);
        return toDestinationValue > fromDestinationValue
            ? NavigationDirection.Forward
            : NavigationDirection.Backward;
    }
}
