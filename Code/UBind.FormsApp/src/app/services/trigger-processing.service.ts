import { Injectable } from "@angular/core";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { TextWithExpressions } from "@app/expressions/text-with-expressions";
import { StringHelper } from "@app/helpers/string.helper";
import { FormType } from "@app/models/form-type.enum";
import { QuoteType } from "@app/models/quote-type.enum";
import { TriggerDisplayConfig } from "@app/models/trigger-display-config";
import { TriggerConfiguration } from "@app/resource-models/configuration/trigger.configuration";
import { ApplicationService } from "./application.service";
import { ConfigService } from "./config.service";
import { SidebarTextElementsService } from "./sidebar-text-elements.service";
import { TriggerService } from "./trigger.service";
import { UserService } from "./user.service";

/**
 * Holds the sort value for a given trigger type
 */
interface TriggerTypeSortValue {
    triggerType: string;
    sortValue: number;
}

/**
 * Is responsible for managing and maintaining the active referral/decline triggers which 
 * come in along with a calculation response.
 */
@Injectable({
    providedIn: 'root',
})
export class TriggerProcessingService {

    /**
     * A list of the TextWithExpression instances associated with the active triggers.
     * We need to keep a reference to these so that we can dispose of them when a new set 
     * of new triggers come in.
     */
    private activeTriggerDisplayConfigTextExpressions: Array<TextWithExpressions>;

    public constructor(
        private configService: ConfigService,
        private expressionDependencies: ExpressionDependencies,
        private triggerService: TriggerService,
        private applicationService: ApplicationService,
        private sidebarTextElementsService: SidebarTextElementsService,
        private userService: UserService,
    ) { }

    public generateActiveTriggerDisplayConfigs(responseTriggers: object): void {
        let newTriggerDisplayConfigs: Array<TriggerDisplayConfig> = new Array<TriggerDisplayConfig>();
        let newExpressions: Array<TextWithExpressions> = new Array<TextWithExpressions>();
        for (const [triggerType, responseTriggersOfType] of Object.entries(responseTriggers)) {
            for (const [triggerName, active] of Object.entries(responseTriggersOfType)) {
                if (active == true) {
                    let triggerDisplayConfig: TriggerDisplayConfig = {
                        name: triggerName,
                        type: triggerType,
                        header: null,
                        title: null,
                        message: null,
                        displayPrice: true,
                        reviewerExplanation: '',
                    };

                    let triggerConfig: TriggerConfiguration
                        = this.configService.triggers && this.configService.triggers[triggerType]
                            ? this.configService.triggers[triggerType][triggerName]
                            : { name: triggerName, type: triggerType };

                    let headerSource: string = this.getSummarySource(triggerConfig);
                    if (headerSource) {
                        let textExpression: TextWithExpressions = this.setupTextExpression(
                            headerSource,
                            `${triggerType} ${triggerName} header expression`,
                            (text: string) => triggerDisplayConfig.header = text);
                        if (textExpression) {
                            newExpressions.push(textExpression);
                        }
                    }

                    let titleSource: string = this.getTitleSource(triggerConfig);
                    if (titleSource) {
                        let textExpression: TextWithExpressions = this.setupTextExpression(
                            titleSource,
                            `${triggerType} ${triggerName} title expression`,
                            (text: string) => triggerDisplayConfig.title = text);
                        if (textExpression) {
                            newExpressions.push(textExpression);
                        }
                    }

                    let messageSource: string = this.getMessageSource(triggerConfig);
                    if (messageSource) {
                        let textExpression: TextWithExpressions = this.setupTextExpression(
                            messageSource,
                            `${triggerType} ${triggerName} title expression`,
                            (text: string) => triggerDisplayConfig.message = text);
                        if (textExpression) {
                            newExpressions.push(textExpression);
                        }
                    }

                    let reviewerExplanationSource: string = this.getReviewerExplanationSource(triggerType, triggerName);
                    if (reviewerExplanationSource) {
                        let textExpression: TextWithExpressions = this.setupTextExpression(
                            reviewerExplanationSource,
                            `${triggerType} ${triggerName} reviewer explanation expression`,
                            (text: string) => triggerDisplayConfig.reviewerExplanation = text);
                        if (textExpression) {
                            newExpressions.push(textExpression);
                        }
                    }

                    triggerDisplayConfig.displayPrice = triggerConfig.displayPrice;
                    newTriggerDisplayConfigs.push(triggerDisplayConfig);
                }
            }
        }

        let oldExpressions: Array<TextWithExpressions> = this.activeTriggerDisplayConfigTextExpressions;
        this.activeTriggerDisplayConfigTextExpressions = newExpressions;
        this.triggerService.activeTriggerDisplayConfigs = newTriggerDisplayConfigs;
        if (oldExpressions) {
            oldExpressions.forEach((e: TextWithExpressions) => e.dispose());
        }
        this.triggerService.activeTrigger = this.getActiveTrigger();
    }

    private getSummarySource(triggerConfig: TriggerConfiguration): string {
        let summary: string = this.userService.isCustomer
            ? triggerConfig.customerSummary
            : triggerConfig.agentSummary
                ? triggerConfig.agentSummary
                : triggerConfig.customerSummary;
        if (summary) {
            return summary;
        }
        if (this.applicationService.formType == FormType.Quote) {
            const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
            return this.sidebarTextElementsService
                .getSidebarTextElementForQuoteType(quoteType, triggerConfig.type + 'TriggeredHeader');
        }
        // TODO: We do not have any default titles for CLAIM triggers. This may change in the future though.
        return null;
    }

    private getTitleSource(triggerConfig: TriggerConfiguration): string {
        let titleSource: string = this.userService.isCustomer
            ? triggerConfig.customerTitle
            : triggerConfig.agentTitle
                ? triggerConfig.agentTitle
                : triggerConfig.customerTitle;
        if (titleSource) {
            return titleSource;
        }
        if (this.applicationService.formType == FormType.Quote) {
            const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
            return this.sidebarTextElementsService
                .getSidebarTextElementForQuoteType(quoteType, triggerConfig.type + 'TriggeredLabel');
        }
        // TODO: We do not have any default titles for CLAIM triggers. This may change in the future though.
        return null;
    }

    private getMessageSource(triggerConfig: TriggerConfiguration): string {
        let messageSource: string = this.userService.isCustomer
            ? triggerConfig.customerMessage
            : triggerConfig.agentMessage
                ? triggerConfig.agentMessage
                : triggerConfig.customerMessage;
        let fallbackMessageSource: any;
        if (!messageSource && this.applicationService.formType == FormType.Quote) {
            const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
            fallbackMessageSource = this.sidebarTextElementsService
                .getSidebarTextElementForQuoteType(quoteType, triggerConfig.type + 'TriggeredMessageDefault');
        }
        // TODO: We do not have any default messages for CLAIM triggers. This may change in the future though.

        messageSource = messageSource || fallbackMessageSource;
        if (messageSource && this.applicationService.formType == FormType.Quote) {
            const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
            let messageAppendix: any = this.sidebarTextElementsService
                .getSidebarTextElementForQuoteType(quoteType, triggerConfig.type + 'TriggeredMessageAppendix');
            if (messageAppendix) {
                messageSource += '<span class="message-appendix"> ' + messageAppendix + '</span>';
            }
        }
        // TODO: We do not have any default messages for CLAIM triggers. This may change in the future though.
        return messageSource;
    }

    private getReviewerExplanationSource(triggerType: string, triggerName: string): string {
        return this.configService.triggers?.[triggerType]?.[triggerName]?.reviewerExplanation;
    }

    private setupTextExpression(
        textSource: string,
        debugIdentifier: string,
        callback: (text: string) => void,
    ): TextWithExpressions {
        if (!StringHelper.isNullOrEmpty(textSource)) {
            let textExpression: TextWithExpressions = new TextWithExpressions(
                textSource,
                this.expressionDependencies,
                debugIdentifier);
            textExpression.nextResultObservable.subscribe((text: string) => callback(text));
            textExpression.triggerEvaluation();
            return textExpression;
        }
        return null;
    }

    /**
     * Gets the first active trigger in the order of precedence for trigger types.
     */
    private getActiveTrigger(): TriggerDisplayConfig {
        let triggerDisplayConfig: TriggerDisplayConfig;
        const triggerTypesInOrderOfPrecedence: Array<string> = this.getTriggerTypesInOrderOfPrecedence();
        for (let triggerType of triggerTypesInOrderOfPrecedence) {
            triggerDisplayConfig = this.triggerService.getFirstActiveTriggerByType(triggerType);
            if (triggerDisplayConfig != null) {
                return triggerDisplayConfig;
            }
        }

        // there must be no active triggers
        return null;
    }

    private getTriggerTypesInOrderOfPrecedence(): Array<string> {
        let triggerTypeSortValues: Array<TriggerTypeSortValue> = new Array<TriggerTypeSortValue>();
        // TODO: one day we should be getting this sort order from the product configuration
        // and there should be a default config json somewhere which it uses if not set.
        triggerTypeSortValues.push({ triggerType: "error", sortValue: 1 });
        triggerTypeSortValues.push({ triggerType: "decline", sortValue: 2 });
        triggerTypeSortValues.push({ triggerType: "hardReferral", sortValue: 3 });
        triggerTypeSortValues.push({ triggerType: "softReferral", sortValue: 4 });
        triggerTypeSortValues.push({ triggerType: "notification", sortValue: 5 });
        triggerTypeSortValues.push({ triggerType: "endorsement", sortValue: 6 });
        triggerTypeSortValues.push({ triggerType: "assessment", sortValue: 7 });
        triggerTypeSortValues.push({ triggerType: "review", sortValue: 8 });

        for (let triggerDisplayConfig of this.triggerService.activeTriggerDisplayConfigs) {
            let hasTriggerTypeSortValue: boolean =
                triggerTypeSortValues.filter((t: TriggerTypeSortValue) =>
                    t.triggerType == triggerDisplayConfig.type).length > 0;
            if (!hasTriggerTypeSortValue) {
                triggerTypeSortValues.push({ triggerType: triggerDisplayConfig.type, sortValue: 1000 });
            }
        }

        let sortedTriggerTypes: Array<TriggerTypeSortValue> = triggerTypeSortValues.sort(
            (a: TriggerTypeSortValue, b: TriggerTypeSortValue) => {
                return (a.sortValue < b.sortValue)
                    ? -1
                    : (a.sortValue == b.sortValue) ? 0 : 1;
            });

        let triggerTypesInOrderOfPrecedence: Array<string> = new Array<string>();
        sortedTriggerTypes.forEach((ttsv: TriggerTypeSortValue) => {
            triggerTypesInOrderOfPrecedence.push(ttsv.triggerType);
        });
        return triggerTypesInOrderOfPrecedence;
    }
}
