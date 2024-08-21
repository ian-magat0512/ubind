import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";
import { ThemeConfiguration } from "@app/resource-models/configuration/theme.configuration";
import { TriggerConfiguration } from "@app/resource-models/configuration/trigger.configuration";
import { QuestionMetadata } from "../question-metadata";
import { QuestionSets } from "./question-sets";
import { Settings } from "./settings";
import { TextElement } from "./text-element";
import { WorkflowStep } from "./workflow-step";

/**
 * The configuration which the webform app works with. This differs from the configuration it receives from a
 * configuration API call. The structure of data is transformed into this working model so that it can be consumed
 * quickly and easily.
 */
export interface WorkingConfiguration {
    version: string;
    privateFieldKeys: Array<string>;
    questionMetadata: {
        questionSets:  {
            [key: string]: {
                [key: string]: QuestionMetadata;
            };
        };
        repeatingQuestionSets:  {
            [key: string]: {
                [key: string]: QuestionMetadata;
            };
        };
    };

    /* ---- For configuration V2 ----- */
    fieldsOrderedByCalculationWorkbookRow: Array<FieldConfiguration>;
    repeatingFieldsOrderedByCalculationWorkbookRow: Array<FieldConfiguration>;
    /* ------------------------------- */

    settings: Settings;
    sidebarSummaryKeyMapping: { [key: string]: string };
    textElements: {
        formElements: {
            helpMessageLabel: TextElement;
            repeatingAddItemButton: TextElement;
            repeatingRemoveItemButton: TextElement;

        };
        sidebar: { [key: string]: TextElement};
        sidebarAdjustment: { [key: string]: TextElement};
        sidebarCancellation: { [key: string]: TextElement};
        sidebarPurchase: { [key: string]: TextElement};
        sidebarRenewal: { [key: string]: TextElement};
        sidebarPanels: { [key: string]: TextElement};
        workflow: {
            [key: string]: TextElementsForStep;
        };
    };
    triggers: {
        [key: /* trigger type */string]: {
            [key: string]: TriggerConfiguration; /* trigger key */
        };
    };
    workflowRoles: { [key: /* workflowRole */string]: /* fieldName */string };
    dataLocators: object;
    questionSets: QuestionSets;
    repeatingQuestionSets: QuestionSets;
    workflowSteps: { [key: string]: WorkflowStep };
    styles: {
        css: string;
    };
    formModel: object;
    calculatesUsingStandardWorkbook: boolean;
    icons: Array<string>;
    optionSets: Map<string, Array<OptionConfiguration>>;
    theme: ThemeConfiguration;
    contextEntities: object;
    repeatingInstanceMaxQuantity: number;
}

/**
 * Represents the text elements for a given step
 */
export interface TextElementsForStep {
    [key: string]: TextElement;
}
