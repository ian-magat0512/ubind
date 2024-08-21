import { SectionDisplayMode } from "@app/models/section-display-mode.enum";

/**
 * Provides status information about a section widget.
 */
export interface SectionWidgetStatus {
    isFirstArticle(): boolean;
    isLastArticle(): boolean;
    getFirstArticleIndex(): number;
    getLastArticleIndex(): number;
    hasNextArticle(): boolean;
    hasPreviousArticle(): boolean;
    getCurrentArticleIndex(): number;
    getNextArticleIndex(): number;
    getPreviousArticleIndex(): number;
    isFirstArticleElement(): boolean;
    isLastArticleElement(): boolean;
    getFirstArticleElementIndex(): number;
    getLastArticleElementIndex(): number;
    hasNextArticleElement(): boolean;
    hasPreviousArticleElement(): boolean;
    getCurrentArticleElementIndex(): number;
    getNextArticleElementIndex(): number;
    getPreviousArticleElementIndex(): number;
    getCurrentWorkflowStepDisplayMode(): SectionDisplayMode;

    getArticleIndexForArticleElementIndex(articleElementIndex: number): number;
}
