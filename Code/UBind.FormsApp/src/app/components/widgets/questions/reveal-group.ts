import { Field } from "@app/components/fields/field";

/**
 * Represents a group of fields to be revealed together
 * This class tracks which groups of fields should be revealed or not
 */
export class RevealGroup {
    private _reveal: boolean = false;
    public fields: Array<Field> = new Array<Field>();

    public get valid(): boolean {
        for (let field of this.fields) {
            if (!field.valid) {
                return false;
            }
        }
        return true;
    }

    public set reveal(reveal: boolean) {
        this._reveal = reveal;
        for (let field of this.fields) {
            field.reveal = reveal;
        }
    }

    public get reveal(): boolean {
        return this._reveal;
    }
}
