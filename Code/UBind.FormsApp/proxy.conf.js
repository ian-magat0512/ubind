const fs = require('fs');
const jsoncParser = require('jsonc-parser');

// load the JSON file with your settings (adjust the path as needed)
const data = fs.readFileSync('../UBind.Web/appsettings.Development.json', 'utf8');

// Parse the data using jsonc-parser
const settings = jsoncParser.parse(data);

// replace 'ApiBaseUrl' with the appropriate key in your appsettings file
const target = settings.InternalUrlConfiguration.BaseApi;

module.exports = {
    '/api': {
        target,
        secure: false,
        logLevel: 'debug'
    },
    '/assets/ubind.js': {
        target,
        secure: false,
        logLevel: 'debug'
    }
};
