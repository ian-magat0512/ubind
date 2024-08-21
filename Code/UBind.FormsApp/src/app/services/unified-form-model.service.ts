import { Injectable } from "@angular/core";
import { ConfigService } from "./config.service";
import * as _ from 'lodash-es';
import { StringHelper } from "@app/helpers/string.helper";
import { ExpressionInputSubjectService } from "@app/expressions/expression-input-subject.service";
import { UnifiedFormModel } from "@app/models/unified-form-model";
import { EventService } from "./event.service";

/**
 * Export unified form model service class.
 * TODO: Write a better class header: unified form model functions.
 */
@Injectable({
    providedIn: 'root',
})
export class UnifiedFormModelService {

    /**
     * The working form model includes values for fields which have been hidden,
     * so that when they are unhidden, they can be given their previous values.
     */
    public workingFormModel: UnifiedFormModel = new UnifiedFormModel();

    /**
     * The strict form model does not include values for fields which are hidden,
     * and it typically used for generating a payload.
     */
    public strictFormModel: UnifiedFormModel = new UnifiedFormModel();

    /**
     * A set of form data, which if exists will overwrite the form data coming in from other places.
     * An example:
     * A freight provider embeds freight insurance on their website. They seed the form model with some data
     * such as the customer's details and consignment number. The customer loads the form and fills it out,
     * changing their phone number (because the one the freight provider sent through in the seed form data
     * was incorrect). The customer then closes the form, and edit's their details and gets a new consignment
     * number. Since we have a new consignment number, we want that to be overwritten in the form model, but
     * not the customer's phone number, since the customer updated that.
     * So the freight provider passes in the consignment number in the "overwriteFormData" property, and
     * here we apply it after each time the form model is set.
     */
    public overwriteFormData: any;

    public constructor(
        private configService: ConfigService,
        private expressionInputSubjectService: ExpressionInputSubjectService,
        private eventService: EventService,
    ) {
    }

    /**
     * Where the product configuration defines a form.model.json file, it will be
     * loaded here, but this should only be called for new quotes, not when
     * we know we are going to load an existing quote.
     */
    public applyInitialFormModelFromConfiguration(): void {
        this.apply(this.configService.formModel);
    }

    /**
     * Loads the given form model, merging the data with the existing
     * unified form model data. 
     * This is typically done during startup when we are loading an existing form,
     * or when we are pre-seeding the form data using javascript.
     * Note that if any question sets have already been rendered, they will NOT have their
     * values updated.
     * @param formModel 
     */
    public apply(formModel: object): void {
        _.merge(this.workingFormModel.model, formModel, this.overwriteFormData);
        _.merge(this.strictFormModel.model, formModel, this.overwriteFormData);
        this.createFieldValueSubjectsForExpressions(this.strictFormModel.model);
    }

    /**
     * Deletes the current form model data, so we start with an empty set.
     * This is used when resuming a quote.
     */
    public clear(): void {
        this.workingFormModel.model = {};
        this.strictFormModel.model = {};
        this.deleteValueSubjectsForExpressions();
    }

    /**
     * When we load in field data, we also need to ensure that field value subjects are created
     * so that expressions can draw upon the values in the form data.
     */
    private createFieldValueSubjectsForExpressions(formModel: object, baseFieldPath: string = null): void {
        for (const [key, value] of Object.entries(formModel)) {
            const fieldPath: string = StringHelper.isNullOrEmpty(baseFieldPath) ? key : `${baseFieldPath}.${key}`;
            if (_.isArray(value)) {
                this.expressionInputSubjectService.getFieldRepeatingCountSubject(fieldPath, value.length);
                for (let i: number = 0; i < value.length; i++) {
                    const instance: any = value[i];
                    if (_.isObject(instance)) {
                        this.createFieldValueSubjectsForExpressions(instance, `${fieldPath}[${i}]`);
                    }
                }
            } else if (_.isObject(value)) {
                this.createFieldValueSubjectsForExpressions(value, fieldPath);
            } else {
                // publish that the fieldPath was added so that we can inspect the config for tags
                this.eventService.fieldPathAddedSubject.next(fieldPath);

                // set the initial value for expressions
                this.expressionInputSubjectService.getFieldValueSubject(fieldPath, value);
            }
        }
    }

    private deleteValueSubjectsForExpressions(): void {
        this.expressionInputSubjectService.deleteAllFieldValueSubjects();
    }
}
