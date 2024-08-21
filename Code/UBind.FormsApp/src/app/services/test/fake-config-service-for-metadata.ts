import { FieldDataType } from "@app/models/field-data-type.enum";
import { QuestionMetadata } from "@app/models/question-metadata";
import { BehaviorSubject } from "rxjs";

/**
 * A fake config service that returns fake metadata, for use in unit tests.
 */
export class FakeConfigServiceForMetadata {
    public configurationReadySubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public questionMetadatas: Array<QuestionMetadata> = new Array<QuestionMetadata>();
    public debug: boolean = false;

    public constructor() {
    }

    public getQuestionMetadata(fieldPath: string): QuestionMetadata {
        let result: QuestionMetadata = this.questionMetadatas.pop();
        if (!result) {
            result = {
                dataType: FieldDataType.Text,
                displayable: true,
                private: false,
                canChangeWhenApproved: true,
                resetForNewQuotes: false,
                resetForNewRenewalQuotes: false,
                resetForNewAdjustmentQuotes: false,
                resetForNewCancellationQuotes: false,
                resetForNewPurchaseQuotes: false,
                tags: [],
                name: '',
            };
        }
        return result;
    }

    public getRepeatingQuestionMetadata(parentKey: string, key: string): QuestionMetadata {
        let result: QuestionMetadata = this.questionMetadatas.pop();
        if (!result) {
            result = {
                dataType: FieldDataType.Currency,
                currencyCode: 'AUD',
                displayable: true,
                private: false,
                canChangeWhenApproved: true,
                resetForNewQuotes: false,
                resetForNewRenewalQuotes: false,
                resetForNewAdjustmentQuotes: false,
                resetForNewCancellationQuotes: false,
                resetForNewPurchaseQuotes: false,
                tags: [],
                name: '',
            };
        }
        return result;
    }

    public addCurrencyMetadataResult(): void {
        let x: QuestionMetadata = {
            dataType: FieldDataType.Currency,
            currencyCode: 'AUD',
            displayable: true,
            private: false,
            canChangeWhenApproved: true,
            resetForNewQuotes: false,
            resetForNewRenewalQuotes: false,
            resetForNewAdjustmentQuotes: false,
            resetForNewCancellationQuotes: false,
            resetForNewPurchaseQuotes: false,
            tags: [],
            name: '',
        };
        this.questionMetadatas.push(x);
    }

    public addBooleanMetadataResult(): void {
        let x: QuestionMetadata = {
            dataType: FieldDataType.Boolean,
            displayable: true,
            private: false,
            canChangeWhenApproved: true,
            resetForNewQuotes: false,
            resetForNewRenewalQuotes: false,
            resetForNewAdjustmentQuotes: false,
            resetForNewCancellationQuotes: false,
            resetForNewPurchaseQuotes: false,
            tags: [],
            name: '',
        };
        this.questionMetadatas.push(x);
    }

    public addTaggedMetadataResult(tags: Array<string>, amount: number): void {
        let x: QuestionMetadata = {
            dataType: FieldDataType.Text,
            displayable: true,
            private: false,
            canChangeWhenApproved: true,
            resetForNewQuotes: false,
            resetForNewRenewalQuotes: false,
            resetForNewAdjustmentQuotes: false,
            resetForNewCancellationQuotes: false,
            resetForNewPurchaseQuotes: false,
            tags: tags,
            name: '',
        };
        for (let i: number = 0; i < amount; i++) {
            this.questionMetadatas.push(x);
        }
    }
}
