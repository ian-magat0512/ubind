import { Entity } from '@app/models/entity';
import { RoleType } from '@app/models/role-type.enum';

/**
 * Role resource model
 */
export interface RoleResourceModel extends Entity {
    name: string;
    description: string;
    type: RoleType;
    isFixed: boolean;
    organisationId: string;
    isPermanentRole: boolean;
    isDeletable: boolean;
    isRenamable: boolean;
    arePermissionsEditable: boolean;
    permissions: Array<RolePermissionResourceModel>;
    isDeleted: boolean;
    createdDateTime: string;
    lastModifiedDateTime: string;
    tenantId: string;
}

/**
 * A role permission resource model
 */
export interface RolePermissionResourceModel {
    type: string;
    description: string;
    concern: string;
}
