import { ProgressStepIcons } from "@app/resource-models/configuration/progress-step-icons";
import { SectionDisplayMode } from "../section-display-mode.enum";
import { WorkflowAction } from "./workflow-action";

/**
 * Represents the configuration for a single workflow step
 */
export interface WorkflowStep {
    startScreen: boolean;
    startScreenExpression: string;
    tabIndex: number;
    tabIndexExpression: string;
    articles: Array<Article>;
    sidebar: Array<SidebarElement>;
    actions: Array<WorkflowAction>;
    header: Array<ContentDefinition>;
    footer: Array<ContentDefinition>;
    displayMode: SectionDisplayMode;
    displayModeExpression: string;
    progressStepIcons: ProgressStepIcons;
    progressStepIcon: string;

    /**
     * When set to true, action buttons can be clicked and executed even if a calculation is in progress.
     * Defaults to false.
     */
    allowActionsWhilstCalculationInProgress: boolean;

    /**
     * When set to true, if the action defines a the "calculation" operation, then it will be skipped if a
     * calculation is in progress already.
     * Defaults to false.
     */
    skipWorkflowStepCalculationWhenOneIsInProgress: boolean;

    /**
     * This is a temporary hack that fixes the problem described in UB-7663, and will be removed
     * once we have more structured workflow definitions.
     */
    updateBackendWithCurrentWorkflowStepWhenItDiffers?: boolean;
}

/**
 * Represents an article in a workflow step
 */
export interface Article {
    name: string;
    heading: string;
    text: string;
    elements: Array<ArticleElement>;
    hiddenExpression: string;
    cssClasses: Array<string>;
    cssClass: string;
}

/**
 * Represents a sidebar element in a workflow step
 */
export interface SidebarElement {
    type: string;
}

/**
 * Represents an article element
 */
export interface ArticleElement {
    type: string;
    name: string;
    hiddenExpression: string;
}

/**
 * Represents the selection fo a question set to be shown within an article
 */
export interface QuestionSet extends ArticleElement {
    affectsPremium: boolean;
    affectsTriggers: boolean;
    requiredForCalculation: boolean;
}

/**
 * Represents a content item within an article
 */
export interface ContentDefinition extends ArticleElement {
    /**
     * The name of a text element where the content should be loaded from.
     * Optional - only required if not specifying the content inline.
     */
    textElement?: string;

    /**
     * The content, which can be text or html.
     */
    content?: string;

    /**
     * A css class, or list of css classes separated by space.
     */
    cssClass?: string;
}
