/* eslint-disable @typescript-eslint/naming-convention */
import * as DOMPurify from 'dompurify';

/**
 * Provides shared security helper functions.
 */
export class SecurityHelper {
    public static getDomPurifyConfig(): DOMPurify.Config {
        return {
            ADD_TAGS: ['my-custom-angular-element'],
            ADD_ATTR: [
                'target',
                'label',
                'content',
            ],
        };
    }
}
