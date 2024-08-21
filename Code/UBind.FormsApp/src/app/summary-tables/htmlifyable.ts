/**
 * Represents something that can be converted to HTML and rendered in a web browser
 */
export interface Htmlifyable {
    toHtml(): string;
}
