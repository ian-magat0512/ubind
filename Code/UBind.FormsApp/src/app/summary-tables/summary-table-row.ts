import { SummaryTableCell } from "./summary-table-cell";
import { SummaryTableEntry } from "./summary-table-entry";

/**
 * Renders a table row to html
 */
export class SummaryTableRow implements SummaryTableEntry {

    public constructor(
        public cells: Array<SummaryTableCell>,
        public cssClasses?: Set<string>,
    ) {
        if (!this.cssClasses) {
            this.cssClasses = new Set<string>();
        }
    }

    public toHtml(): string {
        let classAttribute: string = this.cssClasses && this.cssClasses.size
            ? ` class="${Array.from(this.cssClasses).join(' ')}"`
            : '';
        let html: string = `<tr${classAttribute}>`;
        for (let cell of this.cells) {
            html += cell.toHtml();
        }
        html += '</tr>';
        return html;
    }
}
