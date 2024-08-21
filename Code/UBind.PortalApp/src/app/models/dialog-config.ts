/**
 * Interface for dialog data.
 */
export interface DialogData {
    configuration: DialogConfiguration;
}

/**
 * Interface for dialog options.
 */
export interface DialogConfiguration {
    header: string;
    subheader: string;
    buttons: Array<DialogButton>;
    inputs: Array<DialogInput>;
}

/**
 * Interface for dialog inputs.
 */
export interface DialogInput {
    type?: 'input' | 'radio';
    name?: string;
    placeholder?: string;
    value?: any;
    label?: string;
    checked?: boolean;
    disabled?: boolean;
    id?: string;
    required?: boolean;
    key?: string;
    options?: Array<NameValue<string, any>>;
}

/**
 * Interface for dialog inputs.
 */
export interface NameValue<N, V> {
    name: N;
    value: V;
}

/**
 * Interface for dialog button.
 */
export interface DialogButton {
    label?: string;
    handler?: () => void;
    role?: string;
}
