/**
 * This is necessary to allow it to compile with typescript.
 */
const anyNavigator: any = window.navigator;

/**
 * Provides the ability to copy text to the clipboard.
 */
export class Clipboard {

    public static copyTextToClipboard(text: string): void {
        if (!anyNavigator.clipboard) {
            Clipboard.fallbackCopyTextToClipboard(text);
            return;
        }
        anyNavigator.clipboard.writeText(text).then(() => {
            console.log('Async: Copying to clipboard was successful!');
        }, (err: any) => {
            console.error('Async: Could not copy text: ', err);
        });
    }

    private static fallbackCopyTextToClipboard(text: string): void {
        const textArea: HTMLTextAreaElement = document.createElement('textarea');
        textArea.value = text;

        // Avoid scrolling to bottom
        textArea.style.top = '0';
        textArea.style.left = '0';
        textArea.style.position = 'fixed';

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            const successful: boolean = document.execCommand('copy');
            const msg: string = successful ? 'successful' : 'unsuccessful';
            console.log('Fallback: Copying text command was ' + msg);
        } catch (err) {
            console.error('Fallback: Oops, unable to copy', err);
        }

        document.body.removeChild(textArea);
    }
}
