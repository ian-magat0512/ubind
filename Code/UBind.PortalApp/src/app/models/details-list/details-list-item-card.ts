/**
 * Represents a container of groups of items to be rendered.
 */
export class DetailsListItemCard {
    private name: string;
    private description: string;

    public constructor(name: string, description: string) {
        this.name = name;
        this.description = description;
    }

    public get Name(): string {
        return this.name;
    }

    public get Description(): string {
        return this.description;
    }
}
