import { PortalResourceModel } from "../resource-models/portal/portal.resource-model";
import { LocalDateHelper } from "../helpers";
import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { SortDirection, SortedEntityViewModel } from "./sorted-entity.viewmodel";
import { SortAndFilterBy } from "@app/models/sort-filter-by.enum";
import { PortalUserType } from "@app/models/portal-user-type.enum";

/**
 * Export portal view model class.
 * TODO: Write a better class header: view model of portal.
 */
export class PortalViewModel implements SegmentableEntityViewModel, SortedEntityViewModel {

    public id: string;
    public name: string;
    public alias: string;
    public title: string;
    public deleted: boolean;
    public disabled: boolean;
    public isDefault: boolean;
    public segment: string;
    public createdDate: string;
    public lastModifiedDate: string;
    public tenantId: string;
    public userType: PortalUserType;
    public organisationId: string;
    public productionUrl: string;
    public stagingUrl: string;
    public developmentUrl: string;
    public deleteFromList: boolean;
    public sortByValue: string;
    public sortDirection: SortDirection;

    public constructor(portal: PortalResourceModel) {
        this.id = portal.id;
        this.name = portal.name;
        this.alias = portal.alias;
        this.title = portal.title;
        this.deleted = portal.deleted;
        this.deleteFromList = portal.deleted;
        this.disabled = portal.disabled;
        this.isDefault = portal.isDefault;
        this.createdDate = LocalDateHelper.toLocalDate(portal.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(portal.lastModifiedDateTime);
        this.tenantId = portal.tenantId;
        this.userType = portal.userType;
        this.organisationId = portal.organisationId;
        this.productionUrl = portal.productionUrl;
        this.stagingUrl = portal.stagingUrl;
        this.developmentUrl = portal.developmentUrl;
        this.segment = !this.disabled && !this.deleted ? 'active' : this.disabled ? 'disabled' : '';
        this.sortByValue = this.name;
    }

    public setSortOptions(
        portalList: Array<PortalViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<PortalViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Ascending : sortDirection;

        if (sortBy === SortAndFilterBy.CreatedDate) {
            portalList.forEach((item: PortalViewModel) => {
                item.sortByValue = item.createdDate;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            portalList.forEach((item: PortalViewModel) => {
                item.sortByValue = item.lastModifiedDate;
                item.sortDirection = sortDirection;
            });
        } else {
            portalList.forEach((item: PortalViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        }

        return portalList;
    }
}
