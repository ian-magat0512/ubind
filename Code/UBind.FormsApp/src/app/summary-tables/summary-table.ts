import { Htmlifyable } from "./htmlifyable";
import { SummaryTableEntry } from "./summary-table-entry";

/**
 * Represents an entire Summary Table that is made up of entries and smaller parts.
 * Can be rendered as HTML in a web browser.
 */
export class SummaryTable implements Htmlifyable {

    public constructor(
        public entries: Array<SummaryTableEntry>,
        public cssClasses?: Set<string>,
    ) {
        if (!this.entries) {
            this.entries = new Array<SummaryTableEntry>();
        }
        if (!cssClasses) {
            this.cssClasses = new Set<string>();
        }
        if (!cssClasses.has('summary-table')) {
            this.cssClasses.add('summary-table');
            // so that people can write custom css, they can target this class:
            this.cssClasses.add('custom');
        }
    }

    public toHtml(): string {
        let html: string = `<table class="${Array.from(this.cssClasses).join(' ')}">`;
        for (let entry of this.entries) {
            html += entry.toHtml();
        }
        html += '</table>';
        return html;
    }
}
