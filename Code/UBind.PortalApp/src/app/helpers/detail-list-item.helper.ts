import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailsListItemActionIcon } from "@app/models/details-list/details-list-item-action-icon";
import { StringHelper } from "./string.helper";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { Permission } from "@app/helpers/permissions.helper";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export detail list item helper class
 * This class is for creating of details item list and helper.
 */
export class DetailListItemHelper {
    public static detailListItemIconMap: any = {
        person: "person",
        phone: "call",
        email: "mail",
        others: "folder",
        link: "link",
        calendar: "calendar",
        folder: "folder",
        calculator: "calculator",
        code: "code",
        clearAll: "clear_all",
        list: "list",
        today: "today",
        details: "calculator",
        relationships: "link",
        dates: "calendar",
        template: "code",
        shirt: "shirt",
        cloudCircle: "cloud-circle",
        cube: "cube",
        browsers: "browsers",
        alert: "alert",
        ribbon: "ribbon",
        mail: "mail",
        business: "business",
        deployment: "business",
        cloudUpload: "cloud-upload",
        checkmark: "checkmark-circle",
        brush: "brush",
        sms: "text",
        grid: "grid",
        shield: "shield",
        clipboard: "clipboard",
        linkedIdentities: "card-account-details",
        usage: "albums-outline",
    };

    public static detailListItemDescriptionMap: any = {
        id: "ID",
        cc: "CC",
        bcc: "BCC",
        sourceData: "Data Sources",
        textBody: "Text body",
        mimeType: "Mime-type",
        expiryDate: "Expiry Date",
        expiryTimeOfDay: "Expiry Time",
        claimNumber: "Claim Number",
        assignmentType: "Select Method",
        stylesheetUrl: "Stylesheet URL",
        defaultPortalStylesheetUrl: "Default Portal Stylesheet URL",
        smtpServerHost: "SMTP Host",
        smtpServerPort: "SMTP Port",
        htmlBody: "HTML Body",
        plainTextBody: "Plain Text Body",
        agentOrUserIdentifier: "Agent or User Identifier",
        dnsSelector: "DNS Selector",
        csvData: "CSV Data",
        defaultForProduction: "Default for Production",
        defaultForStaging: "Default for Staging",
        quotes: "Quotes",
        policyTransactions: "Policy Transactions",
    };

    public static createDetailItemGroup(
        group: DetailsListItemCardType,
        modelArray: Array<DetailsListGroupItemModel>,
        icon: string = null,
        iconLibrary: string = IconLibrary.IonicV4,
        header: string = "",
        includeSectionIcons: boolean = true,
        convertCamelToSentenceCase: boolean = true,
    ): Array<DetailsListItem> {
        let items: Array<DetailsListItem> = [];
        let card: DetailsListItemCard =
            new DetailsListItemCard(group, group);
        if (!icon) {
            icon = this.detailListItemIconMap[StringHelper.camelCase(group)];
        }
        let firstItem: boolean = true;
        modelArray.forEach((model: DetailsListGroupItemModel) => {
            let descriptionMap: string = this.detailListItemDescriptionMap[model.identifier];
            let description: string = descriptionMap
                ? descriptionMap
                : convertCamelToSentenceCase
                    ? StringHelper.camelToSentenceCase(model.identifier)
                    : model.identifier;
            let detailListItem: DetailsListItem = DetailsListItem.createItem(
                card,
                group,
                model.value,
                description,
                icon,
                iconLibrary,
                header,
                model.maxLines,
                model.lineCount,
                includeSectionIcons,
                model.isMultiLine);
            if (model.value) {
                if (model.actions) {
                    detailListItem = Array.isArray(model.actions)
                        ? detailListItem.withActions(...model.actions)
                        : detailListItem.withAction(model.actions);
                    if (model.relatedEntityType) {
                        detailListItem = detailListItem.withRelatedEntity(
                            model.relatedEntityType,
                            model.relatedEntityOrganisationId,
                            model.relatedEntityOwnerId,
                            model.relatedEntityCustomerId);
                    }
                }
                let isRoundIcon: boolean =
                    DetailsListItemCardType.Relationships == group && firstItem;
                if (isRoundIcon) {
                    detailListItem = detailListItem.roundIcon();
                }
                items.push(detailListItem);
                firstItem = false;
            }
        });
        return items;
    }

    public static createAction(
        callback?: () => void,
        icon: string = 'link',
        iconLibrary: string = IconLibrary.IonicV4,
        mustHavePermissions?: Array<Permission>,
        mustHaveOneOfPermissions?: Array<Permission>,
        mustHaveOneOfEachSetOfPermissions?: Array<Array<Permission>>,
    ): DetailsListItemActionIcon {
        let action: DetailsListItemActionIcon = new DetailsListItemActionIcon([icon], [iconLibrary]);
        if (callback) {
            action.ClickEventObservable.subscribe(($event: Event) => {
                callback();
            });
        }

        action.mustHavePermissions = mustHavePermissions;
        action.mustHaveOneOfPermissions = mustHaveOneOfPermissions;
        action.mustHaveOneOfEachSetOfPermissions = mustHaveOneOfEachSetOfPermissions;
        return action;
    }
}
