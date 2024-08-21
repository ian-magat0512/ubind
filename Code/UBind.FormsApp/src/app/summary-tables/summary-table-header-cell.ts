import { SummaryTableCell } from "./summary-table-cell";

/**
 * A table cell in a SummaryTable that is part of a table header e.g. th
 */
export class SummaryTableHeaderCell extends SummaryTableCell {

    public constructor(
        contents: string,
        cssClasses: Set<string>,
        colspan?: number,
    ) {
        super(contents, cssClasses, colspan);
        this.tag = 'th';
    }
}
