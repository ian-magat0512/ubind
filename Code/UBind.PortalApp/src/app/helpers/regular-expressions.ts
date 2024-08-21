/**
 * Export Regular Expressions class
 * Manage the regexp to used like password filtering.
 * @dynamic
 */
export class RegularExpressions {
    // Matches phone numbers in the format:
    //   0xxxxxxxxx or
    //   +61xxxxxxxx,
    // with whitespace permitted anywhere.
    public static australianPhoneNumber: RegExp = /^\s*(0|\+\s*6\s*1)(\s*\d){9}\s*$/;

    // Matches phone numbers in the format:
    //   04xxxxxxxx or
    //   +614xxxxxxx,
    // with whitespace permitted anywhere.
    public static australianMobilePhoneNumber: RegExp = /^\s*(0|\+\s*6\s*1)\s*(4|5)(\s*\d){8}\s*$/;

    // Matches passwords consisting of at least 12 characters and including at least one each of:
    //   upper case letters
    //   lower case letters
    //   digits
    //   any other character (not a letter or digit)
    public static strongPassword: RegExp = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$/;

    // Matches valid generic aliases
    //     alias1
    //     portal-tenant44
    // Alias must only contain lowercase alphabetic characters, digits and hyphens. 
    // It can begin with numbers.
    // It must not end or begin with a hyphen.
    // It must not null.
    public static alias: RegExp = /^(?!.*\bnull\b)(?=.*[a-z])[a-z0-9][a-z]*(?:-?[a-z0-9]+)*$/;

    // Matches valid generic entity name
    //     Entity Name
    //     Entity Name001
    //     Entity001Name
    //     Entity001Name -'. ,
    // Entity name must only contain alphabetic characters, space, digits and hyphens. 
    // It can begin with numbers.
    // It must not end or begin with a hyphen.
    public static entityName: RegExp = /^(?!.*\bnull\b)(?=.*[a-zA-Z])[a-zA-Z0-9](?:-?[a-zA-Z0-9. ,']+)*$/;

    // Matches valid alphanumeric characters with spaces
    //     role1
    //     12organisation name12
    // Matches alphanumeric characters with space alone.
    public static alphaNumericWithSpace: RegExp = /(^[a-zA-Z0-9 ]*$)/;

    // Matches valid Email Separated by Semicolon
    //     sample-email@domain.com;test@test.com;
    public static emailSeparatedBySemiColonPattern: RegExp = /^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$/;

    // Matches valid Email with name
    // test@domain.com
    // test user <test@domain.com>
    // test-user <test@domain.com>
    // https://regex101.com/r/j0KUlS/1
    public static emailWithNamePattern: RegExp = /^((?=.*?<|>)^([\w-_\s])+\s*<([\w-]+@[\w-]+(?:\.[\w-]+)+)>s*$|^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$)+?$/;

    // Matches valid Smtp Server Host
    //     127.0.0.1
    public static smtpServerHost: RegExp = /^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$/;

    // Matches valid Domain name
    //  google.com
    //  app.ubind.com.au
    //  localhost
    public static domainName: RegExp = /^localhost|((?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$/;

    // Matches valid Smtp Server Port number
    //     80
    public static smtpServerPort: RegExp = /^([1-9]|[12]\d|6553[0-5])$/;

    // Matches valid Whole Number or Interger
    //     123
    //     256
    // It must only contain numerical characters and no decimal
    public static wholeNumber: RegExp = /^-?(0|[1-9]\d*)?$/;

    // Matches valid Whole Number or Interger
    //     123
    //     256
    //     -1 -> error
    // It must only contain numerical characters and no decimal and below zero
    public static postiveWholeNumber: RegExp = /^[1-9]\d{0,6}$/;

    public static positiveNumber: RegExp = /^[+]?([.]\d+|\d+([.]\d+)?)/;
    // Matches valid currency
    // 123.23
    // 123.2
    // 123.
    // 123
    // .23
    // .2
    public static currency: RegExp = /^\d*\.?(?:\d{1,2})?$/;

    // Matches valid currency with sign
    // $123.23
    public static currencyWithSign: RegExp = /^\$?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$/;

    // Matches valid currency with optional commas and decimal (up to 2)
    // 1,000.00
    // 1,000
    // 1000
    // 1000.00
    public static fullCurrency: RegExp = /^(\d+|\d{1,3}(,\d{3})*)(\.\d{1,2})?$/;

    // Matches only letter characters and a few special characters
    // It must contain only letter characters and a few special characters
    //     Firstname Lastname IV'.,-
    public static naming: RegExp = new RegExp("^([a-zA-Z]+[-'. ,]*)+$");

    // Matches only letters, numbers and spaces and dash
    // aaa bbbb ccc
    public static customLabel: RegExp = new RegExp("^[a-zA-Z0-9\\-\\s]+$");

    // Matches All alpha-numeric characters and selected special characters
    // Allowed special characters = '~?!&+%:$-@\/(),
    public static company: RegExp = new RegExp("^([a-zA-Z '?!&+%:$-@\/\\\\\(\),]*)+$");

    // Matches All alpha-numeric characters, spaces and selected special characters
    // Allowed special characters = '-,.
    public static streetAddress: RegExp = new RegExp("^([a-zA-Z0-9\\'\\-\\,\\. ])+$");

    // Matches letter characters and spaces
    public static addressSuburb: RegExp = new RegExp("^[a-zA-Z\\s]+$");

    // Matches numeric characters with a length of 4 characters
    public static addressPostcode: RegExp = new RegExp("^([0-9]{4,4})+$");

    // Matches URL web address url (but not FTP)
    public static webUrl: RegExp = /^(?:(?:(?:https?):)?\/\/)?(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:localhost)|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?/i;

    // Matches URL string like website, ip address, other supported url formats including FTP
    public static url: RegExp = /^(?:(?:(?:https?|ftp):)?\/\/)?(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?/i;

    // Matches valid claim number values with letters, numbers, and the following special characters: ,.:;-()
    // C-0000
    // UBcAXd
    // A
    // 1
    // (uB)-0001:05;03.6,7
    public static claimNumber: RegExp = /^([a-zA-Z0-9\,\.\:\;\-\(\)]+)$/;

    public static timeAmPm: RegExp = /((1[0-2]|0?[1-9]):([0-5][0-9]) ([AaPp][Mm]))/;

    public static timeMilitary: RegExp = /^([01]\d|2[0-3]):?([0-5]\d)$/;

    public static stylesheetUrl: RegExp = /^(https:\/\/|http:\/\/)(((\*|[\w\d]+(-[\w\d]+)*)\.)*([-\w\d]+)(\.\w{1,4})(\.\w{0,4})?|localhost)(\:(\d{1,5}))?(\/\w+){0,10}(\w+.css)((\?))?(\w+=\w+(\&)?){0,5}$/;

    public static policyNumber: RegExp = /^([a-zA-Z0-9\,\.\:\;\-\(\)]+)$/;
}
