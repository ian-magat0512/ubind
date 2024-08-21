/**
 * Represents a resource model of product deployment settings,
 * which says where a web form can be rendered (into which websites)
 */
export class ProductDeploymentSettingResourceModel {

    public constructor() {
        this.development = [];
        this.staging = [];
        this.production = [];
    }

    public developmentIsActive: boolean;
    public productionIsActive: boolean;
    public stagingIsActive: boolean;
    public development: Array<string>;
    public production: Array<string>;
    public staging: Array<string>;
}
