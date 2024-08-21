/**
 * Configuration about where ubind forms can be embedded (which website domains).
 */
export interface ProductDeploymentSettings {
    development: Array<string>;
    staging: Array<string>;
    production: Array<string>;
    developmentIsActive: boolean;
    stagingIsActive: boolean;
    productionIsActive: boolean;
}
