import { } from 'jasmine';
import { RegularExpressions } from './regular-expressions';

describe('RegularExpressions.AustralianPhoneNumber', () => {
    it('matches 10 digit numbers beginning with 0', () => {
        expect(RegularExpressions.australianPhoneNumber.test('03 1234 1234')).toBe(true);
    });

    it('matches 11 digit numbers beginning with +61', () => {
        expect(RegularExpressions.australianPhoneNumber.test('+61 3 1234 1234')).toBe(true);
    });

    it('does not match 10 digit numbers beginning with +61', () => {
        expect(RegularExpressions.australianPhoneNumber.test('+61 3 1234 123')).toBe(false);
    });

    it('does not match matches 11 digit numbers beginning with 0', () => {
        expect(RegularExpressions.australianPhoneNumber.test('03 1234 12345')).toBe(false);
    });

    it('does not match matches strings with letters', () => {
        expect(RegularExpressions.australianPhoneNumber.test('0x 1234 1234')).toBe(false);
    });

    it('does not match matches strings with parentheses', () => {
        expect(RegularExpressions.australianPhoneNumber.test('(03) 1234 1234')).toBe(false);
    });
});

describe('RegularExpressions.AustralianMobilePhoneNumber', () => {

    it('matches 10 digit numbers beginning with 04', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('04 1234 1234')).toBe(true);
    });

    it('matches 10 digit numbers beginning with 05', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('05 1234 1234')).toBe(true);
    });

    it('matches 11 digit numbers beginning with +614', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('+61 4 1234 1234')).toBe(true);
    });

    it('matches 11 digit numbers beginning with +615', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('+61 5 1234 1234')).toBe(true);
    });

    it('does not match 10 digit numbers beginning with +614', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('+61 4 1234 123')).toBe(false);
    });

    it('does not match matches 11 digit numbers beginning with 04', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('04 1234 12345')).toBe(false);
    });

    it('does not match matches 10 digit numbers beginning with 03', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('03 1234 12345')).toBe(false);
    });

    it('does not match matches strings with letters', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('0x 1234 1234')).toBe(false);
    });

    it('does not match matches strings with parentheses', () => {
        expect(RegularExpressions.australianMobilePhoneNumber.test('(03) 1234 1234')).toBe(false);
    });
});

describe('RegularExpressions.StrongPassword', () => {
    it(
        'matches valid password with at least one uppercase letter, lowercase letter, digit and special character.',
        () => {
            expect(RegularExpressions.strongPassword.test('#EDC4rfv%TGB')).toBe(true);
        },
    );

    it('matches valid password where only special character is in first place', () => {
        expect(RegularExpressions.strongPassword.test('#23456789o1Z')).toBe(true);
    });

    it('matches valid password where only digit is in first place', () => {
        expect(RegularExpressions.strongPassword.test('1ZEASGf&qOIZ')).toBe(true);
    });

    it('matches valid password where only lowercase letter is in first place', () => {
        expect(RegularExpressions.strongPassword.test('l234567&9O12')).toBe(true);
    });

    it('matches valid password where only uppercase letter is in first place', () => {
        expect(RegularExpressions.strongPassword.test('Iz34567&9012')).toBe(true);
    });

    it('does not match strings with only 11 characters', () => {
        expect(RegularExpressions.strongPassword.test('!234567B9o1')).toBe(false);
    });

    it('does not match strings with no special characters', () => {
        expect(RegularExpressions.strongPassword.test('EDC4rfv1TGBs')).toBe(false);
    });

    it('does not match strings with no uppercase characters', () => {
        expect(RegularExpressions.strongPassword.test('#edc4rfv%tgb')).toBe(false);
    });

    it('does not match strings with no lowercase characters', () => {
        expect(RegularExpressions.strongPassword.test('#EDC4RFV%TGB')).toBe(false);
    });

    it('does not match strings with no digits', () => {
        expect(RegularExpressions.strongPassword.test('#EDCsRFV%TGB')).toBe(false);
    });
});

describe('RegularExpressions.Alias', () => {
    it('matches aliases that contain only lowercase alphabetic characters, digits and hyphen', () => {
        expect(RegularExpressions.alias.test('product1-name123')).toBe(true);
    });

    it('matches aliases that contain only alphabetic characters', () => {
        expect(RegularExpressions.alias.test('productname')).toBe(true);
    });

    it('matches aliases that contains digits', () => {
        expect(RegularExpressions.alias.test('alias123456')).toBe(true);
    });

    it('matches aliases that starts with digits and contain at least one alphabetic character', () => {
        expect(RegularExpressions.entityName.test('123456a')).toBe(true);
    });

    it('matches aliases that has hypen', () => {
        expect(RegularExpressions.entityName.test('123456-a')).toBe(true);
    });

    it('does not match aliases that end with a hyphen', () => {
        expect(RegularExpressions.alias.test('product123-')).toBe(false);
    });

    it('does not match aliases that start with a hyphen', () => {
        expect(RegularExpressions.alias.test('-product123')).toBe(false);
    });

    it('does not match aliases that contain special symbols other than hyphen', () => {
        expect(RegularExpressions.alias.test('product@123')).toBe(false);
    });

    it('does not match aliases that is the word \'null\'', () => {
        expect(RegularExpressions.entityName.test('null')).toBe(false);
    });

    it('does not match aliases without alphabetic characters', () => {
        expect(RegularExpressions.entityName.test('12345')).toBe(false);
    });
});

describe('RegularExpressions.EntityName', () => {
    it('matches strings with only contain letters, spaces, hyphens, apostrophes, commas and period characters', () => {
        expect(RegularExpressions.entityName.test('Product1 Name -,.\' 123')).toBe(true);
    });

    it('matches entity name that contain only alphabetic characters', () => {
        expect(RegularExpressions.entityName.test('Productname')).toBe(true);
    });

    it('matches entity name that contain digits', () => {
        expect(RegularExpressions.entityName.test('alias123456')).toBe(true);
    });

    it('matches entity name that starts with digits and contain at least one alphabetic character', () => {
        expect(RegularExpressions.entityName.test('123456a')).toBe(true);
    });

    it('matches entity name that has hypen', () => {
        expect(RegularExpressions.entityName.test('123456-a')).toBe(true);
    });

    it('does not match entity name that end with a hyphen', () => {
        expect(RegularExpressions.entityName.test('product123-')).toBe(false);
    });

    it('does not match entity name that start with a hyphen', () => {
        expect(RegularExpressions.entityName.test('-product123')).toBe(false);
    });

    it('does not match entity name without alphabetic characters', () => {
        expect(RegularExpressions.entityName.test('12345')).toBe(false);
    });

    it('does not match entity name that contain special symbols other than spaces, hyphens,' +
        ' apostrophes, commas and period', () => {
        expect(RegularExpressions.entityName.test('product@123')).toBe(false);
    });

    it('does not match entity name that is the word \'null\'', () => {
        expect(RegularExpressions.entityName.test('null')).toBe(false);
    });
});

describe('RegularExpressions.Name', () => {
    it('matches strings with only contain letters, spaces, hyphens, apostrophes, commas and period characters', () => {
        expect(RegularExpressions.naming.test('Firstname Lastname III\'.,')).toBe(true);
    });

    it('must start with a letter, and may only contain letters, '
        + 'spaces, hyphens, apostrophes, commas and period characters', () => {
        expect(RegularExpressions.naming.test('a\'.,\'')).toBe(true);
    });

    it('does not match string with special characters other than specified in the regex like @ symbol', () => {
        expect(RegularExpressions.naming.test('itmatchesThisString@@\'.,')).toBe(false);
    });

    it('does not match string with special characters other than specified in the regex like / symbol', () => {
        expect(RegularExpressions.naming.test('firstname lastname III\'.,/')).toBe(false);
    });

    it('does not match string with number like 2 ', () => {
        expect(RegularExpressions.naming.test('firstname lastname III\'.,2')).toBe(false);
    });

    it('does not match string with other special characters not specified in the regex', () => {
        expect(RegularExpressions.naming.test('!@#@)))~~~/!@#')).toBe(false);
    });
});

describe('RegularExpressions.AlphaNumericWithSpace', () => {
    it('matches text that contain only lowercase alphabetic characters, digits and space', () => {
        expect(RegularExpressions.alphaNumericWithSpace.test('product1 name123')).toBe(true);
    });

    it('matches text that contain only alphabetic characters', () => {
        expect(RegularExpressions.alphaNumericWithSpace.test('productname')).toBe(true);
    });

    it('matches text that contain only digits', () => {
        expect(RegularExpressions.alphaNumericWithSpace.test('123456')).toBe(true);
    });

    it('does not match texts that end with a hyphen', () => {
        expect(RegularExpressions.alphaNumericWithSpace.test('product123-')).toBe(false);
    });

    it('does not match texts that start with a hyphen', () => {
        expect(RegularExpressions.alias.test('-product123')).toBe(false);
    });

    it('does not match texts that contain special symbols other than hyphen', () => {
        expect(RegularExpressions.alias.test('product@123')).toBe(false);
    });
});

describe(
    'RegularExpressions.ClaimNumber matches strings with letters, numbers and the following symbols , . : ; - ( )',
    () => {
        it('Must match strings with letters, numbers and hyphens like C-000-4', () => {
            expect(RegularExpressions.claimNumber.test('C-000-4')).toBe(true);
        });

        it('Must match strings with parentheses and colons like (ub):0004', () => {
            expect(RegularExpressions.claimNumber.test('(ub):0004')).toBe(true);
        });

        it('Must match strings with periods and commas like UB.004,', () => {
            expect(RegularExpressions.claimNumber.test('UB.004,')).toBe(true);
        });

        it('Must not match strings with a space like (ub): 0004', () => {
            expect(RegularExpressions.claimNumber.test('(ub): 0004')).toBe(false);
        });

        it('Must not match strings with symbols !@#$%^&*', () => {
            expect(RegularExpressions.claimNumber.test('!!!!UBC@#$%^&*004')).toBe(false);
        });
    },
);

describe('RegularExpressions.StylesheetUrl', () => {
    it('must match valid stylesheet url', () => {
        expect(RegularExpressions.stylesheetUrl.test('http://google.com:123/css.css')).toBe(true);
        expect(RegularExpressions.stylesheetUrl.test('http://google.com:123/css.css?')).toBe(true);
        expect(RegularExpressions.stylesheetUrl.test('http://google.com/qweww.css?qwe=qwe')).toBe(true);
        expect(RegularExpressions.stylesheetUrl.test('http://localhost/qwosxow/qwoexqwe/qweww.css?qwe=qwe')).toBe(true);
    });

    it('must not match an invalid stylesheet url', () => {
        expect(RegularExpressions.stylesheetUrl.test('https://google.com.ph/thisisnotacss.html')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('this is not a url')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('thisisnotaurl')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('123w12e!@#!@#')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('localhost://123')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('noprotocol.com.ph/style.css')).toBe(false);
        expect(RegularExpressions.stylesheetUrl.test('google.com.ph?test=true')).toBe(false);
    });
});

describe('RegularExpressions.URL', () => {
    it('must match valid website URL', () => {
        expect(RegularExpressions.url.test('www.example.com')).toBe(true);
        expect(RegularExpressions.url.test('https://example.com')).toBe(true);
        expect(RegularExpressions.url.test('https://www.website.com/')).toBe(true);
        expect(RegularExpressions.url.test('http://www.website.com/path/to/page')).toBe(true);
        expect(RegularExpressions.url.test('www.subdomain.example.com')).toBe(true);
        expect(RegularExpressions.url.test('//website.com/path/to/page')).toBe(true);
    });

    it('must not match an invalid website URL', () => {
        expect(RegularExpressions.url.test('plainaddress')).toBe(false);
        expect(RegularExpressions.url.test('tel:45212136')).toBe(false);
        expect(RegularExpressions.url.test('/path/to/page')).toBe(false);
        expect(RegularExpressions.url.test('sms:45212136')).toBe(false);
        expect(RegularExpressions.url.test('javascript:alert(\'hello\');')).toBe(false);
    });
});

describe('RegularExpressions.StreetAddress', () => {
    it('must match valid street address', () => {
        expect(RegularExpressions.streetAddress.test('My simple address')).toBe(true);
        expect(RegularExpressions.streetAddress.test('P.O. Box 42 Huslia, AK 99746')).toBe(true);
    });

    it('must not match an invalid street address', () => {
        expect(RegularExpressions.streetAddress.test('47/675 WODONGA DRIVE')).toBe(false);
        expect(RegularExpressions.streetAddress.test('C/O John Paul, POBox 456, Motown, CA 96090')).toBe(false);
        expect(RegularExpressions.streetAddress.test('######')).toBe(false);
        expect(RegularExpressions.streetAddress.test('!!!!@@@@@')).toBe(false);
    });
});

describe('RegularExpressions.AddressSuburb', () => {
    it('must match valid address suburb', () => {
        expect(RegularExpressions.addressSuburb.test('South Brisbane')).toBe(true);
        expect(RegularExpressions.addressSuburb.test('Brisbane')).toBe(true);
        expect(RegularExpressions.addressSuburb.test('Victoria')).toBe(true);
        expect(RegularExpressions.addressSuburb.test('Queensland')).toBe(true);
        expect(RegularExpressions.addressSuburb.test('Albert Park')).toBe(true);
    });

    it('must not match an invalid address suburb', () => {
        expect(RegularExpressions.addressSuburb.test('St Kilda West, Melbourne, Victoria')).toBe(false);
    });
});

describe('RegularExpressions.AddressPostcode', () => {
    it('must match valid address postcode', () => {
        expect(RegularExpressions.addressPostcode.test('0000')).toBe(true);
        expect(RegularExpressions.addressPostcode.test('2600')).toBe(true);
        expect(RegularExpressions.addressPostcode.test('9100')).toBe(true);
    });

    it('must not match an invalid address postcode', () => {
        expect(RegularExpressions.addressPostcode.test('000')).toBe(false);
        expect(RegularExpressions.addressPostcode.test('00011')).toBe(false);
        expect(RegularExpressions.addressPostcode.test('23223')).toBe(false);
    });
});

describe('RegularExpressions.DomainName', () => {
    it('must match valid domain name', () => {
        expect(RegularExpressions.domainName.test('google.com')).toBe(true);
        expect(RegularExpressions.domainName.test('www.google.com')).toBe(true);
        expect(RegularExpressions.domainName.test('localhost')).toBe(true);
    });

    it('must not match an invalid domain name', () => {
        expect(RegularExpressions.domainName.test('google')).toBe(false);
        expect(RegularExpressions.domainName.test('123')).toBe(false);
        expect(RegularExpressions.domainName.test('invalid domain name')).toBe(false);
        expect(RegularExpressions.domainName.test('/path/to/page')).toBe(false);
        expect(RegularExpressions.domainName.test('https://www.google.com/')).toBe(false);
    });
});

describe(
    'RegularExpressions.PolicyNumber matches strings with letters, numbers and the following symbols , . : ; - ( )',
    () => {
        it('Must match strings with letters, numbers and hyphens like C-000-4', () => {
            expect(RegularExpressions.policyNumber.test('P-000-4')).toBe(true);
        });

        it('Must match strings with parentheses and colons like (ub):0004', () => {
            expect(RegularExpressions.policyNumber.test('(ub):0004')).toBe(true);
        });

        it('Must match strings with periods and commas like UB.004,', () => {
            expect(RegularExpressions.policyNumber.test('UB.004,')).toBe(true);
        });

        it('Must not match strings with a space like (ub): 0004', () => {
            expect(RegularExpressions.policyNumber.test('(ub): 0004')).toBe(false);
        });

        it('Must not match strings with symbols !@#$%^&*', () => {
            expect(RegularExpressions.policyNumber.test('!!!!UBC@#$%^&*004')).toBe(false);
        });
    });
