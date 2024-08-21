export interface DataTableSchema {
    columns: Array<Column>;
    clusteredIndex: ClusteredIndex | null;
    unclusteredIndexes: Array<UnclusteredIndex> | null;
}

export interface ClusteredIndex {
    name: string;
    alias: string;
    keyColumns: Array<KeyColumn>;
}

export interface Column {
    name: string;
    alias: string;
    dataType: string;
    defaultValue: string | null;
    required: boolean | null;
    unique: boolean | null;
}

export interface UnclusteredIndex extends ClusteredIndex {
    nonKeyColumns: Array<string> | null;
}

export interface KeyColumn {
    columnAlias: string;
    sortOrder: "Asc" | "Desc";
}
