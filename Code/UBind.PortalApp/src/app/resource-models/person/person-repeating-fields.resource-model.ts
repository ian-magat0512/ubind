import { EmailAddressFieldResourceModel } from "./email-address-field.resource-model";
import { MessengerIdFieldResourceModel } from "./messenger-id-field.resource-model";
import { PhoneNumberFieldResourceModel } from "./phone-number-field.resource-model";
import { SocialMediaIdFieldResourceModel } from "./social-field.resource-model";
import { StreetAddressFieldResourceModel } from "./street-address-field.resource-model";
import { WebsiteAddressFieldResourceModel } from "./website-address-field.resource-model";

/**
 * Resource model for person set of repeating fields
 */
export interface PersonRepeatingFieldsResourceModel  {
    emailAddresses: Array<EmailAddressFieldResourceModel>;
    phoneNumbers: Array<PhoneNumberFieldResourceModel>;
    streetAddresses: Array<StreetAddressFieldResourceModel>;
    websiteAddresses: Array<WebsiteAddressFieldResourceModel>;
    messengerIds: Array<MessengerIdFieldResourceModel>;
    socialMediaIds: Array<SocialMediaIdFieldResourceModel>;
}
