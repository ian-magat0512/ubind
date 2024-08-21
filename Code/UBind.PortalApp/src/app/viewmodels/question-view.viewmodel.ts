/**
 * Export question view model class.
 * TODO: Write a better class header: view model of question.
 */
export class QuestionViewModel {
    public questionLabel: string;
    public questionValue: string;
    public isAttachment: boolean;
}

/**
 * Export question attachment view model class
 * TODO: Write a better class header: view model of question attachment.
 */
export class QuestionAttachmentViewModel extends QuestionViewModel {

    public constructor() {
        super();
    }

    public attachmentId: string;
    public attachmentName: string;
}
