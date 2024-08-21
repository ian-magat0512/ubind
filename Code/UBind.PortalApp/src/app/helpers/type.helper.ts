import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import {
    AuthenticationMethodResourceModel,
    LocalAccountAuthenticationMethodResourceModel,
    SamlAuthenticationMethodResourceModel,
} from "@app/resource-models/authentication-method.resource-model";
import {
    PortalLocalAccountLoginMethodResourceModel,
    PortalLoginMethodResourceModel,
    PortalSamlLoginMethodResourceModel,
} from "@app/resource-models/portal/portal-login-method.resource-model";

/**
 * Provides typescript type guards to check what the type of an object is.
 */
export class TypeHelper {

    public static isLocalAccountAuthenticationMethodResourceModel(
        model: AuthenticationMethodResourceModel,
    ): model is LocalAccountAuthenticationMethodResourceModel {
        return model.typeName == AuthenticationMethodType.LocalAccount;
    }

    public static isSamlAuthenticationMethodResourceModel(
        model: AuthenticationMethodResourceModel,
    ): model is SamlAuthenticationMethodResourceModel {
        return model.typeName == AuthenticationMethodType.Saml;
    }

    public static isPortalLocalAccountLoginMethodResourceModel(
        model: PortalLoginMethodResourceModel,
    ): model is PortalLocalAccountLoginMethodResourceModel {
        return model.typeName == AuthenticationMethodType.LocalAccount;
    }

    public static isPortalSamlLoginMethodResourceModel(
        model: PortalLoginMethodResourceModel,
    ): model is PortalSamlLoginMethodResourceModel {
        return model.typeName == AuthenticationMethodType.Saml;
    }

}
