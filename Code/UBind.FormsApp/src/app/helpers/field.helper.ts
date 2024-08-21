import { FieldType } from "@app/models/field-type.enum";

/**
 * Utilities for working with field paths
 */
export class FieldHelper {
    public static getFieldElement(fieldType: FieldType, nativeElement: HTMLElement): any {
        if (fieldType == FieldType.DropDownSelect) {
            return nativeElement.getElementsByTagName('select')[0];
        } else {
            return nativeElement.getElementsByTagName('input')[0];
        }
    }

    public static getNextElementToFocus(fieldPath: string, fieldType: FieldType): HTMLElement {
        const inputElements: Array<HTMLInputElement> = this.getTabbableElements(fieldPath, fieldType);
        const activeElement: Element = this.getActiveElement(fieldPath, fieldType);
        let currentIndex: number = inputElements.findIndex((e: HTMLInputElement) => e === activeElement);
        let targetFocusElement: HTMLElement = null;
        // Get the next element skipping the elements within the same group field
        do {
            targetFocusElement = inputElements[++currentIndex];
        } while (targetFocusElement && activeElement.id &&
            targetFocusElement.id.split('-')[0] === activeElement.id.split('-')[0]);
        return targetFocusElement;
    }

    public static getTabbableElements(fieldPath: string, fieldType: FieldType): Array<HTMLInputElement> {
        let selector: string = '.tabbable, .search-select input';
        if (fieldType == FieldType.Attachment) {
            selector += `, #${fieldPath}`;
        }
        return Array.from(document.querySelectorAll(selector)) as Array<HTMLInputElement>;
    }

    public static getActiveElement(fieldPath: string, fieldType: FieldType): Element {
        const getByIdFields: Array<FieldType> = [
            FieldType.Attachment,
            FieldType.DatePicker];
        return getByIdFields.includes(fieldType)
            ? document.getElementById(`${fieldPath}`)
            : document.activeElement;
    }

    public static isElementVisible(element: HTMLElement): boolean {
        const rect: DOMRect = element.getBoundingClientRect();
        return (
            rect.width > 0 &&
            rect.height > 0 &&
            window.getComputedStyle(element).display !== 'none'
        );
    }

    public static delayFocusUntilVisible(targetElement: HTMLElement) {
        return new Promise<void>((resolve: (value: void | PromiseLike<void>) => void) => {
            if (this.isElementVisible(targetElement)) {
                targetElement.focus();
                resolve();
            } else {
                const observer: MutationObserver = new MutationObserver(() => {
                    if (this.isElementVisible(targetElement)) {
                        observer.disconnect();
                        targetElement.focus();
                        resolve();
                    }
                });
                observer.observe(document.body, { subtree: true, childList: true });
            }
        });
    }
}
