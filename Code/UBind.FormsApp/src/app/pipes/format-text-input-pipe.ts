import { PipeTransform } from "@angular/core";

/**
 * Abstract class for a pipe that formats text input
 */
export abstract class FormatTextInputPipe implements PipeTransform  {

    public abstract transform(value: any): string;
    public abstract restore(value: any): string;

    /**
     * Cleans the value entered.
     * @param value the original value
     * @returns the cleaned value
     * Override to clean specific to the field data type.
     */
    public clean(value: string): string {
        return value;
    }
}
