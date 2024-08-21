import { Htmlifyable } from "./htmlifyable";

/**
 * Represents a single cell within a Summary Table, that can be rendered to html.
 */
export class SummaryTableCell implements Htmlifyable {
    protected tag: string = 'td';

    public constructor(
        public contents: string,
        public cssClasses: Set<string>,
        public colspan?: number,
    ) {
        if (!this.cssClasses) {
            this.cssClasses = new Set<string>();
        }
    }

    public toHtml(): string {
        let colspanHtml: string = this.colspan ? ` colspan="${this.colspan}"` : '';
        let cssClassesHtml: string =
            this.cssClasses && this.cssClasses.size ? ` class="${Array.from(this.cssClasses).join(' ')}"` : '';
        return `<${this.tag}${colspanHtml}${cssClassesHtml}>${this.contents}</${this.tag}>`;
    }
}
