import { FieldConfiguration } from "./field.configuration";

/**
 * Represents the configuration for a webhook field, which when triggered will send a request to an external URL
 * and process it's response, optionally setting data in the form.
 */
export interface WebhookFieldConfiguration extends FieldConfiguration {
    /**
     * Gets or sets an expression which evaluates to the URL the webhook should hit.
     */
    urlExpression: string;

    /**
     * Gets or sets an expression which when it's resulting value changes, it will trigger the webhook request.
     */
    triggerExpression: string;

    /**
     * Gets or sets an expression which when it evaluates to true, it will allow webhook requests to be triggered.
     */
    conditionExpression: string;

    /**
     *  Gets or sets the HTTP verb to use when requesting data from the API (GET or POST). Defaults to POST if not
     *  set.
     */
    httpVerb: string;

    /**
     *  Gets or sets a uBind expression which generates some data to send as part of the option request.
     *  This could JSON or XML. This should only be used If the HTTP verb is POST. If using GET, encode the
     *  data in the URL instead.
     *  If not set, the webhook will send the current form model as the payload.
     */
    payloadExpression: string;

    /**
     * Gets or sets the number of milliseconds of no activity to wait before triggering the webhook.
     *
     * If the triggerExpression result changes many times in succession, it can cause many webhook requests to trigger,
     * flooding the API with requests. To stop this from happening after trigger, we have a delay period, during which
     * we wait until there a no further triggers before making the request to the server. Only if there are no further
     * triggers in this period, do we make the request. The default is 100 milliseconds.
     */
    debounceTimeMilliseconds: number;

    /**
     * Gets or sets a value indicating whether when the webhook returns a result, it should automatically populate 
     * any form field values which have matching names.
     */
    autoPopulateFormModel?: boolean;

    /**
     * Gets or sets a value indicating whether when the field is first rendered, whether it should automatically
     * trigger. Defaults to false.
     */
    autoTrigger?: boolean;

    /**
     * Specifies the maximum age in seconds for which the resource can be cached.
     * 
     */
    allowCachingWithMaxAgeSeconds?: number;
}
