/**
 * Contains the logic for navigating through URL.
 */
export class UrlHelper {
    // these are the get params from a url that the portal uses.
    public static portalParamList: Array<string> = [
        "environment",
        "login",
        "type",
        "segment",
        "path",
        "filter",
        "invitationId",
        "productAlias",
        "claimId",
        "quoteId",
        "policyId",
        "product",
        "selectId",
        "selectedId",
        "listsegment",
        "previous",
    ];

    // appends only the new parameter
    // and returns only the query path.
    public static getNewQueryPath(newPath: string, paramName: string, paramValue: string): string {
        if (paramName == "environment") {
            paramValue = paramValue.toLowerCase();
        }
        let origin: string = window.location.origin + window.location.pathname;
        let completePath: string = origin + newPath;
        newPath = UrlHelper.appendNewQueryStringParameterToUrl(completePath, paramName, paramValue);
        newPath = newPath.replace(origin, '');

        // create an exception. convert the encoded characters to the unencoded character.
        newPath = newPath.replace(/%2F/g, "/"); // converts %2F characters to /
        return newPath;
    }

    // The query params from the map will be reduced to params not usually used as parameters.
    public static getQueryParamsNotUsedByThePortal(params: Map<string, string>): Map<string, string> {
        this.portalParamList.forEach((portalParamKey: string) => {
            params.forEach((paramValue: string, paramKey: string) => {
                if (paramKey.toLowerCase() == portalParamKey.toLowerCase()) {
                    params.delete(paramKey);
                }
            });
        });
        return params;
    }

    /**
     * Gathers the parameters from an query path
     * Ex. /test/test?something=something
     * would result in 
     * [{'path':'/test/test'},{'something','something'}]
     */
    public static gatherQueryParamsFromIncompletePath(path: string): Map<string, string> {
        let newParams: Map<string, string> = new Map();
        let separatePathAndOtherParams: Array<string> = path.split('?');
        let itemIndex: number = 0;
        separatePathAndOtherParams.forEach((item: string) => {
            if (item) {
                // possibly a path.
                if (itemIndex == 0) {
                    newParams.set("path", decodeURIComponent(item));
                } else {
                    const parsedUrl: URL = new URL(origin + "?" + item);
                    parsedUrl.searchParams.forEach((value: string, key: string) => {
                        newParams.set(key, value);
                    });
                }
            }
            itemIndex++;
        });
        return newParams;
    }

    /*
    * appends new GET parameter to URL.
    */
    public static appendNewQueryStringParameterToUrl(path: string, paramName: string, paramValue: string) {
        const parsedUrl: URL = new URL(path);
        let decode: string = decodeURIComponent(paramValue);
        parsedUrl.searchParams.set(paramName, decode);
        return parsedUrl.toString();
    }
}
