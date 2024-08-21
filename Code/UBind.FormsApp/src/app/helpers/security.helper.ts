/* eslint-disable @typescript-eslint/naming-convention */
import * as DOMPurify from 'dompurify';

/**
 * Provides shared security helper functions.
 */
export class SecurityHelper {
    public static getDomPurifyConfig(): DOMPurify.Config {
        return {
            ADD_TAGS: ['ubind-action-widget', 'ubind-price-widget', 'ubind-tooltip-widget'],
            ADD_ATTR: [
                'target',
                'action-name',
                'location',
                'sub-location',
                'show-sign',
                'icon',
                'label',
                'content',
                'show-icon',
                'icon-position',
            ],
        };
    }
}
