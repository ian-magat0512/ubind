/**
 * Represents something that can be hidden, and therefore maintains a hidden state.
 */
export interface Hideable {
    setHidden(hidden: boolean): void;
    isHidden(): boolean;
}
