/**
 * Export checkbox class view model.
 * TODO: Write a better class header: Checkbox view model.
 */
export class Checkbox {
    public id: string;
    public name: string;
    public value: boolean;
    public constructor(id: string, name: string, value: boolean) {
        this.id = id;
        this.name = name;
        this.value = value;
    }
}
