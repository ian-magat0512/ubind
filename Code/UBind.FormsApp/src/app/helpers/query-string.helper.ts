import * as JSON5 from 'json5';
import * as _ from 'lodash-es';

/**
 * Export query string helper.
 * This class manage query string functions.
 */
export class QueryStringHelper {

    public static queryStringToJson(queryString: any): object {
        let pairs: any = queryString.split('&');

        let result: any = {};
        pairs.forEach((pair: any) => {
            pair = pair.split('=');
            if (!isNaN(pair[1])) {
                result[pair[0]] = Number(pair[1]);
            } else if (pair[1].startsWith('{') || pair[1].startsWith('[')) {
                result[pair[0]] = JSON5.parse(pair[1]);
            } else {
                result[pair[0]] = decodeURIComponent(pair[1] || '');
            }
        });
        return result;
    }

    public static isQueryString(queryString: any): boolean {
        if (!_.isString(queryString)) {
            return false;
        }
        let matches: RegExpMatchArray = queryString.match(/^[a-zA-Z]+[a-zA-Z0-9]*=/);
        return matches && matches.length > 0;
    }
}
