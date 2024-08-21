import { ArticleElement, ContentDefinition, QuestionSet } from "@app/models/configuration/workflow-step";

/**
 * Provides type checking facilities for ArticleElement subtypes
 */
export class ArticleElementHelper {
    public static isQuestionSet(articleElement: ArticleElement): articleElement is QuestionSet {
        return articleElement.type == 'questions';
    }

    public static isContent(articleElement: ArticleElement): articleElement is ContentDefinition {
        return articleElement.type == 'content';
    }
}
