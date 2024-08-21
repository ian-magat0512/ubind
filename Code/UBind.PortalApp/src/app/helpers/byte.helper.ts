/**
 * Export byte helper class
 * helps out regarding byte related. 
 */
export class ByteHelper {
    public static base64ToArrayBuffer(base64: string): Uint8Array {
        let binaryString: string = window.atob(base64);
        let binaryLen: number = binaryString.length;
        let bytes: Uint8Array = new Uint8Array(binaryLen);
        for (let i: number = 0; i < binaryLen; i++) {
            let ascii: number = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    }

    public static downloadByteArray(fileName: string, byte: Uint8Array, type: string): void {
        let blob: Blob = new Blob([byte], { type: type });
        let link: HTMLAnchorElement = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.target = '_blank';
        link.setAttribute('target', '_blank');
        link.setAttribute("download", fileName);
        link.click();
    }
}
