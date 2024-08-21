import { ConfigurationFileResourceModel } from "@app/models";
import { FormType } from "@app/models/form-type.enum";

/**
 * Represents a configuration file from a product component configuration in a release.
 */
export class ConfigurationFileViewModel {

    public fileName: string;
    public path: string;
    public sourceType: string;
    public resourceUrl: string;
    public formType: FormType;
    public isBrowserViewable: boolean;
    public isDownloading: boolean;

    public constructor(resourceModel: ConfigurationFileResourceModel) {
        this.path = resourceModel.path;
        this.sourceType = resourceModel.sourceType;
        this.resourceUrl = resourceModel.resourceUrl;
        this.formType = resourceModel.formType;
        this.isBrowserViewable = resourceModel.isBrowserViewable;
        this.fileName = this.getFilenameFromPath(this.path);
    }

    private getFilenameFromPath(path: string): string {
        const parts: Array<string> = path.split('/');
        return parts[parts.length - 1];
    }
}
