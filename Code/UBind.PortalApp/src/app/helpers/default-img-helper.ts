/**
 * Export default image helper class
 * Setting and filtering of image source.
 */
export class DefaultImgHelper {
    public static setImageSrcAndFilter(imgElement: any, srcPath: string, styleFilter: string): void {
        imgElement.src = srcPath;
        imgElement.style.filter = styleFilter;
    }
}
