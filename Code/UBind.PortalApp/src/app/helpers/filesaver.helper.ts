import { saveAs } from 'file-saver';

/**
 * Saves a file by opening file-save-as dialog in the browser
 * using file-save library.
 * @param blobContent file content as a Blob
 * @param fileName name the file should be saved as
 */
export const saveFile = (blobContent: Blob, fileName: string, type: string): void => {
    const blob: Blob = new Blob([blobContent], { type: type });
    saveAs(blob, fileName);
};

export const getFilenameFromContentDisposition = (contentDisposition: string): string => {
    let filename: string = "";
    if (contentDisposition?.indexOf('attachment') != -1) {
        const filenameRegex: RegExp = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
        const matches: RegExpExecArray = filenameRegex.exec(contentDisposition);
        if (matches != null && matches[1]) {
            filename = matches[1].replace(/['"]/g, '');
        }
        return filename;
    }
};
