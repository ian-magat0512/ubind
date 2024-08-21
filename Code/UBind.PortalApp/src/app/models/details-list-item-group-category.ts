/**
 * Export details list item group category class.
 * TODO: Write a better class header: Item group category list functions.
 */
export class DetailsListItemGroupCategory {
    private groupCategoryName: string;
    private groupCategoryDescription: string;

    public constructor(name: string, description: string) {
        this.groupCategoryName = name;
        this.groupCategoryDescription = description;
    }

    public get GroupCategoryName(): string {
        return this.groupCategoryName;
    }

    public get GroupCategoryDescription(): string {
        return this.groupCategoryDescription;
    }
}
