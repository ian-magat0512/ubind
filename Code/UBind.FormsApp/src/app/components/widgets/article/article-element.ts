import { Expression } from "@app/expressions/expression";

/**
 * Properties for an article element, which can represent a question set or just text content
 */
export interface ArticleElement {
    definition: any;
    name: string;
    key: string;
    type: string;
    affectsPremium: boolean;
    affectsTriggers: boolean;
    requiredForCalculation: boolean;
    render: boolean;
    hiddenExpression: Expression;
    /**
     * The index of this article element within the section
     */
    sectionArticleElementIndex: number;

    /**
     * The name of the step/page the article element belongs to
     */
    stepName: string;

    /**
     * If the display mode is ArticleElement, then this determines whether this
     * can be displayed.
     */
    canDisplay: boolean;
}
