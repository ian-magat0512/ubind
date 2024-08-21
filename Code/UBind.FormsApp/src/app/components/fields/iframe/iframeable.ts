import { SafeResourceUrl } from "@angular/platform-browser";

/**
 * Something that can be presented in an IFrame in a web browser
 */
export interface Iframeable {
    /**
     * The parent/containing element
     */
    facade: any;

    /**
     * The URL of the page contents to be displayed in the iframe
     */
    url: SafeResourceUrl;
}
