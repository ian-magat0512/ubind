import { QuestionViewModel } from "./question-view.viewmodel";

/**
 * Export repeating question view model class.
 * TODO: Write a better class header: view model of repeating question.
 */
export class RepeatingQuestionViewModel {
    public constructor() { }
    public repeatingQuestionItemKey: string;
    public repeatingQuestionItemDetails: Array<QuestionViewModel>;
}
