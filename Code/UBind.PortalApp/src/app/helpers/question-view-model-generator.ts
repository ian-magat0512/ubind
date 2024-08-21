import { QuestionViewModel, QuestionAttachmentViewModel } from "@app/viewmodels/question-view.viewmodel";
import { JsonParser } from "@app/helpers/jsonparser.helper";
import { DisplayableFieldHelper } from "@app/helpers/displayable-field.helpers";
import { RepeatingQuestionViewModel } from "@app/viewmodels/repeating-question.viewmodel";
import { DisplayableFieldsModel } from "@app/models";

/**
 * This class is needed because the functions can be reused in 
 * multiple parts of the system (like policy, quote and claims)
 * to generate a QuestionViewModel object from a FormData object supplied in the parameter, 
 * otherwise these functions would be repeatedly defined.
 * Also contains functions and variables that will be used in the questions tab display.
 */
export class QuestionViewModelGenerator {
    public static type: any = {
        Quote: 'Quote',
        Policy: 'Policy',
        Claim: 'Claim',
        ClaimVersion: 'Claim-Version',
    };

    public static getFormDataQuestionItems(
        formData: any,
        questionAttachmentKeys: Array<string>,
        displayableFieldsModel: DisplayableFieldsModel,
    ): Array<QuestionViewModel> {
        const cleanData: any = JsonParser.cleanData(formData);
        const nonRepeatingData: any = JsonParser.getFormDataWithoutRepeatingData(cleanData);
        return this.processQuestionData(nonRepeatingData, questionAttachmentKeys, displayableFieldsModel);
    }

    public static getQuestionSetItems(
        questionSet: any,
        questionAttachmentKeys: Array<string>,
        displayFields: any,
    ): Array<QuestionViewModel> {
        const cleanData: any = JsonParser.cleanData(questionSet);
        const nonRepeatingQuestionSets: any = JsonParser.getQuestionSetWithoutRepeatingData(cleanData);

        const qvModels: Array<QuestionViewModel> = [];
        for (const header in nonRepeatingQuestionSets) {
            qvModels.push(
                ...this.processQuestionData(nonRepeatingQuestionSets[header], questionAttachmentKeys, displayFields));
        }

        return qvModels;
    }

    public static getRepeatingData(
        formData: any,
        displayableFieldsModel: DisplayableFieldsModel,
        questionAttachmentKeys: Array<string>,
    ): Array<RepeatingQuestionViewModel> {
        const cleanData: any = JsonParser.cleanData(formData);
        const repeatingData: any = JsonParser.getRepeatingDataFromFormData(cleanData);

        return this.generateRepeatingDataObjectArray(
            repeatingData,
            displayableFieldsModel,
            questionAttachmentKeys,
        );
    }

    public static getRepeatingQuestionSets(
        questionSet: any,
        displayableFieldsModel: DisplayableFieldsModel,
        questionAttachmentKeys: Array<string>,
    ): Array<RepeatingQuestionViewModel> {
        const repeatingData: any = JsonParser.getRepeatingDataFromQuestionSet(questionSet);
        return this.generateRepeatingDataObjectArray(
            repeatingData,
            displayableFieldsModel,
            questionAttachmentKeys,
        );
    }

    private static processQuestionData(
        data: any,
        questionAttachmentKeys: Array<string>,
        displayFields: any,
    ): Array<QuestionViewModel> {
        const qvModels: Array<QuestionViewModel> = [];
        for (const key in data) {
            const fieldValue: any = data[key];
            if (typeof fieldValue === 'string' && this.isDisplayableField(key, displayFields)) {
                const [attachmentName, ,attachmentId]: any = fieldValue.split(':');
                if (questionAttachmentKeys.includes(key) && attachmentId) {
                    const model: QuestionAttachmentViewModel =
                        this.getQuestionViewWithAttachment(attachmentId, attachmentName, key);
                    qvModels.push(model);
                } else {
                    const model: QuestionViewModel = this.getQuestionView(key, fieldValue);
                    qvModels.push(model);
                }
            }
        }

        return qvModels;
    }

    private static getQuestionViewWithAttachment(
        attachmentId: string,
        attachmentName: string,
        key: string,
    ): QuestionAttachmentViewModel {
        return {
            attachmentId,
            attachmentName,
            questionLabel: this.beautify(key),
            questionValue: attachmentName,
            isAttachment: true,
        };
    }

    private static getQuestionView(
        key: string,
        value: string,
    ): QuestionViewModel {
        return {
            questionLabel: this.beautify(key),
            questionValue: value,
            isAttachment: false,
        };
    }

    private static hasAttachment(
        questionAttachmentKeys: Array<string>,
        repeatingDataItemKeyValue: string,
        additionalFileAttachment: string,
    ): boolean {
        return questionAttachmentKeys?.includes(repeatingDataItemKeyValue)
            || questionAttachmentKeys?.includes(additionalFileAttachment);
    }

    private static generateRepeatingDataObjectArray(
        repeatingData: any,
        displayableFieldsModel: DisplayableFieldsModel,
        questionAttachmentKeys: Array<string>,
    ): Array<RepeatingQuestionViewModel> {
        let repeatingDataObject: Array<RepeatingQuestionViewModel> = [];
        for (let repeatingDataItem of this.objectKeys(repeatingData)) {
            if (this.isRepeatingDisplayableField(repeatingDataItem, displayableFieldsModel)) {
                const repeatingDataItems: Array<string> = this.objectKeys(repeatingData[repeatingDataItem]);
                repeatingDataItems.forEach((repeatingDataItemKey: string, repeatingDataItemIndex: number) => {
                    let repeatingDataObjectItems: Array<any> = [];
                    const repeatingDataItemValues: Array<string> =
                        this.objectKeys(repeatingData[repeatingDataItem][repeatingDataItemKey]);
                    repeatingDataItemValues.forEach((repeatingDataItemValue: string, index: number) => {
                        const additionalFileAttachment: string =
                            `${repeatingDataItem}[${index}].${repeatingDataItemValue}`;
                        if (!this.isRepeatingDisplayableField(repeatingDataItemValue, displayableFieldsModel)) {
                            return;
                        }
                        const itemValue: string =
                            repeatingData[repeatingDataItem][repeatingDataItemKey][repeatingDataItemValue];
                        if (this.hasAttachment(
                            questionAttachmentKeys, repeatingDataItemValue, additionalFileAttachment)) {
                            const [attachmentName, ,attachmentId]: any = itemValue.split(':');
                            const model: QuestionAttachmentViewModel = this.getQuestionViewWithAttachment(
                                attachmentId,
                                attachmentName,
                                repeatingDataItemValue,
                            );
                            repeatingDataObjectItems.push(model);
                        } else {
                            const model: QuestionViewModel = this.getQuestionView(repeatingDataItemValue, itemValue);
                            repeatingDataObjectItems.push(model);
                        }

                    });
                    repeatingDataObject.push({
                        repeatingQuestionItemKey: `${this.beautify(repeatingDataItem)} ${repeatingDataItemIndex + 1}`,
                        repeatingQuestionItemDetails: repeatingDataObjectItems,
                    });
                });
            }
        }

        return repeatingDataObject;
    }

    private static isDisplayableField(field: string, displayableFieldsModel: DisplayableFieldsModel): boolean {
        return !displayableFieldsModel.displayableFieldsEnabled ||
            DisplayableFieldHelper.isDisplayableField(field, displayableFieldsModel);
    }

    private static isRepeatingDisplayableField(field: string, displayableFieldsModel: DisplayableFieldsModel): boolean {
        return !displayableFieldsModel.repeatingDisplayableFieldsEnabled ||
            DisplayableFieldHelper.isRepeatingDisplayableField(field, displayableFieldsModel);
    }

    private static beautify(key: string): string {
        if (key) {
            key = key.replace(/([A-Z]+)/g, ' $1').trim();
            key = key.replace(/([0-9]+)/g, ' $1').trim();
            key = key.charAt(0).toUpperCase() + key.slice(1);

            // Handle acronyms to add space character
            const acronyms: RegExpMatchArray = key.match(/(?:[A-Z]){2,}[a-z]/g);
            for (let i: number = 0; acronyms && i < acronyms.length; i++) {
                const word: string = acronyms[i];
                key = key.replace(word, word.slice(0, word.length - 2) + ' ' + word.slice(-2));
            }

            key = key.replace(/  +/g, ' ');
        }

        return key;
    }

    private static objectKeys(data: any): Array<string> {
        if (data) {
            return Object.keys(data);
        }
        return data;
    }
}
