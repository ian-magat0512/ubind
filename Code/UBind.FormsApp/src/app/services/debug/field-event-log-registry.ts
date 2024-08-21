import { Injectable } from "@angular/core";

/**
 * A registry of field paths that require event logs to be output to the console
 */
@Injectable({
    providedIn: 'root',
})
export class FieldEventLogRegistry {
    public fieldPaths: Array<string> = new Array<string>();
}
