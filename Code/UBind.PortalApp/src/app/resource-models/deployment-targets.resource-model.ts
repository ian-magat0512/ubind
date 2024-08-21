/**
 * Resource model for a deployment target, which says which website domains have have
 * a particular uBind forms injected into them.
 */
export interface DeploymentTargetsResourceModel {
    id: string;
    url: string;
    isDeleted: boolean;
    tenant: string;
}
