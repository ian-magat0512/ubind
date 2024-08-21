import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { titleCase } from 'title-case';
import * as ChangeCase from 'change-case';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Config model for additional property pages
 */
export interface AdditionalPropertyCategoryConfig {
    entityType: EntityType;
    icon: string;
    iconLibrary: string;
    isRoundIcon: boolean;
    applicableContext: Array<AdditionalPropertyDefinitionContextType>;
    entityTypeDescription: string;
}

export const additionalPropertyCategories: Array<AdditionalPropertyCategoryConfig> = [
    {
        entityType: EntityType.Customer,
        icon: 'person',
        iconLibrary: IconLibrary.IonicV5,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Customer)),
    },
    {
        entityType: EntityType.Quote,
        icon: 'calculator',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Quote)),
    },
    {
        entityType: EntityType.NewBusinessQuote,
        icon: 'calculator-add',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.NewBusinessQuote)),
    },
    {
        entityType: EntityType.AdjustmentQuote,
        icon: 'calculator-pen',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.AdjustmentQuote)),
    },
    {
        entityType: EntityType.RenewalQuote,
        icon: 'calculator-refresh',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.RenewalQuote)),
    },
    {
        entityType: EntityType.CancellationQuote,
        icon: 'calculator-ban',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.CancellationQuote)),
    },
    {
        entityType: EntityType.QuoteVersion,
        icon: 'calculator',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.QuoteVersion)),
    },
    {
        entityType: EntityType.Policy,
        icon: 'shield',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Policy)),
    },
    {
        entityType: EntityType.PolicyTransaction,
        icon: 'shield',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.PolicyTransaction)),
    },
    {
        entityType: EntityType.NewBusinessPolicyTransaction,
        icon: 'shield-add',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.NewBusinessPolicyTransaction)),
    },
    {
        entityType: EntityType.AdjustmentPolicyTransaction,
        icon: 'shield-pen',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.AdjustmentPolicyTransaction)),
    },
    {
        entityType: EntityType.RenewalPolicyTransaction,
        icon: 'shield-refresh',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.RenewalPolicyTransaction)),
    },
    {
        entityType: EntityType.CancellationPolicyTransaction,
        icon: 'shield-ban',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.CancellationPolicyTransaction)),
    },
    {
        entityType: EntityType.Claim,
        icon: 'clipboard',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Claim)),
    },
    {
        entityType: EntityType.ClaimVersion,
        icon: 'clipboard',
        iconLibrary: IconLibrary.AngularMaterial,
        isRoundIcon: true,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.ClaimVersion)),
    },
    {
        entityType: EntityType.Tenant,
        icon: 'cloud-circle',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Tenant)),
    },
    {
        entityType: EntityType.Organisation,
        icon: 'business',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Organisation)),
    },
    {
        entityType: EntityType.Invoice,
        icon: 'add-circle-outline',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Invoice)),
    },
    {
        entityType: EntityType.CreditNote,
        icon: 'remove-circle-outline',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.CreditNote)),
    },
    {
        entityType: EntityType.Payment,
        icon: 'add-circle',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Payment)),

    },
    {
        entityType: EntityType.Refund,
        icon: 'remove-circle',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Refund)),
    },
    {
        entityType: EntityType.Product,
        icon: 'cube',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Product,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Product)),
    },
    {
        entityType: EntityType.Portal,
        icon: 'browsers',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.Portal)),
    },
    {
        entityType: EntityType.User,
        icon: 'contact',
        iconLibrary: IconLibrary.IonicV4,
        isRoundIcon: false,
        applicableContext: [
            AdditionalPropertyDefinitionContextType.Tenant,
            AdditionalPropertyDefinitionContextType.Organisation,
        ],
        entityTypeDescription: titleCase(ChangeCase.sentenceCase(EntityType.User)),
    },
];
