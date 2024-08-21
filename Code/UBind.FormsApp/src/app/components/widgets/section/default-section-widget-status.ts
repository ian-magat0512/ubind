import { SectionDisplayMode } from "@app/models/section-display-mode.enum";
import { SectionWidgetStatus } from "./section-widget-status";

/**
 * Provides a section widget status which can be used before a real section widget
 * status becomes available.
 * This is needed because during first load of a web form, because, expression methods might be called
 * to inspect the section widget status before one has finished rendering.
 */
export class DefaultSectionWidgetStatus implements SectionWidgetStatus {
    public isFirstArticle(): boolean {
        return true;
    }

    public isLastArticle(): boolean {
        return true;
    }

    public getFirstArticleIndex(): number {
        return -1;
    }

    public getLastArticleIndex(): number {
        return -1;
    }

    public hasNextArticle(): boolean {
        return false;
    }

    public hasPreviousArticle(): boolean {
        return false;
    }

    public getCurrentArticleIndex(): number {
        return -1;
    }

    public getNextArticleIndex(): number {
        return -1;
    }

    public getPreviousArticleIndex(): number {
        return -1;
    }

    public isFirstArticleElement(): boolean {
        return true;
    }

    public isLastArticleElement(): boolean {
        return true;
    }

    public getFirstArticleElementIndex(): number {
        return -1;
    }

    public getLastArticleElementIndex(): number {
        return -1;
    }

    public hasNextArticleElement(): boolean {
        return false;
    }

    public hasPreviousArticleElement(): boolean {
        return false;
    }

    public getCurrentArticleElementIndex(): number {
        return -1;
    }

    public getNextArticleElementIndex(): number {
        return -1;
    }

    public getPreviousArticleElementIndex(): number {
        return -1;
    }

    public getCurrentWorkflowStepDisplayMode(): SectionDisplayMode {
        return SectionDisplayMode.Page;
    }

    public getArticleIndexForArticleElementIndex(articleElementIndex: number): number {
        return -1;
    }
}
