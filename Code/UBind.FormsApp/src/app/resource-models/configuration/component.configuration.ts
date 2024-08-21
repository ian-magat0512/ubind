import { FormConfiguration } from "./form.configuration";
import { TriggerConfiguration } from "./trigger.configuration";

/**
 * Represents the configuration for a component of a product
 */
export interface ComponentConfiguration {
    version: string;
    form: FormConfiguration;
    triggers: Array<TriggerConfiguration>;
    paymentConfiguration: object;
    paymentFormConfiguration: object;
    dataLocators: object;
    calculatesUsingStandardWorkbook: boolean;
    contextEntities: object;
}
