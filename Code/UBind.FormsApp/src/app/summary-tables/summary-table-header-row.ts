import { SummaryTableEntry } from "./summary-table-entry";
import { SummaryTableHeaderCell } from "./summary-table-header-cell";
import { SummaryTableRow } from "./summary-table-row";

/**
 * A row within a Summary Table.
 */
export class SummaryTableHeaderRow implements SummaryTableEntry {

    public constructor(protected headings: Array<string>) {
    }

    public toHtml(): string {
        let cells: Array<SummaryTableHeaderCell> = new Array<SummaryTableHeaderCell>();
        for (let i: number = 0; i < this.headings.length; i++) {
            let cssClass: string = i == 0 ? 'summary-name' : 'summary-value';
            cells.push(new SummaryTableHeaderCell(this.headings[i], new Set<string>([cssClass])));
        }
        let row: SummaryTableRow = new SummaryTableRow(cells);
        return row.toHtml();
    }
}
