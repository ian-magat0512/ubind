import { Field } from "@app/components/fields/field";
import { RepeatingField } from "@app/components/fields/repeating/repeating.field";
import { QuestionsWidget } from "@app/components/widgets/questions/questions.widget";
import { CalculationAffectingElement } from "@app/models/calculation-affecting-element";
import { FieldType } from "@app/models/field-type.enum";

/**
 * The type helper helps to determine what the actual type of the object is.
 * It makes use of typescript type guards where appropriate.
 */
export class TypeHelper {
    public static isObject(object: any): boolean {
        if (object == null) {
            return false;
        }
        return Object.prototype.toString.call(object) == '[object Object]';
    }

    public static isRepeatingField(field: Field): field is RepeatingField {
        return field.fieldType == FieldType.Repeating;
    }

    public static isQuestionsWidget(
        questionsWidget: CalculationAffectingElement,
    ): questionsWidget is QuestionsWidget {
        return (questionsWidget as QuestionsWidget).questionSetPath !== undefined;
    }

    public static isField(field: CalculationAffectingElement): field is Field {
        return (field as Field).fieldType !== undefined;
    }
}
