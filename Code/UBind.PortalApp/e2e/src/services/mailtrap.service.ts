import { resolveDefinition } from "@angular/core/src/view/util";
import { browser } from "protractor";
import { AnyMxRecord } from "dns";

const fileStream = require('fs');
const request = require('request');

export class MailTrapService {

    token: string;
    endpoint: string = 'https://mailtrap.io/';
    public dateTimeNow;

    public constructor(token, dateTimeNow) {
        this.token = token;
        this.dateTimeNow = dateTimeNow - 300000;//minus 5min
    }

    demosLatitudeMotorLoanEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        //filter messages
        if (messageObj.subject != 'Your insurance policy')
            return false;
        if (dateTimeNow > Date.parse(messageObj.created_at))
            return false;

        //filter message body
        if (messageObj.bodyText) {
            if (data.expectedEmailValues.emailSourceHashCode == messageObj.html_body_size
                && data.expectedEmailValues.attachmentSize == messageObj.human_size
                && messageObj.bodyText.indexOf('Thank you for choosing to protect your loan with Latitude Financial. Please find attached your Tax Invoice and Policy Schedule')
                && messageObj.bodyText.indexOf('We strongly recommend that you examine the document carefully to ensure that all the details are correct. If there is a change in your circumstances or if you want to change any details of your cover, please contact us immediately to advise.')
                //check files attached
                && contentRaw.indexOf('Financial Services Guide.pdf')
                && contentRaw.indexOf('Policy Schedule.pdf')
                && contentRaw.indexOf('Product Disclosure Statement.pdf')
                && contentRaw.indexOf('Tax Invoice.pdf'))
                return true;
        }

        return true;
    }

    australianReliancePropertyClubEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        //filter messages
        if (messageObj.subject != 'Your landlords insurance policy')
            return false;
        if (dateTimeNow > Date.parse(messageObj.created_at))
            return false;

        //filter message body
        if (messageObj.bodyText) {
            if (data.expectedEmailValues.emailSourceHashCode == messageObj.html_body_size
                && data.expectedEmailValues.attachmentSize == messageObj.human_size
                && messageObj.bodyText.indexOf("Thank you for choosing to insure your property with 'Property Club Insurance' trading as PSC Insurance Brokers")
                && messageObj.bodyText.indexOf('Attached you will find the following documentation for your attention')
                && messageObj.bodyText.indexOf('If you have any queries in relation to your insurance, or require any amendments to you policy, please do not hesitate to contact us on the details below.')
                //check files attached
                && contentRaw.indexOf('PSC ANSW Financial Services Guide.pdf')
                && contentRaw.indexOf('Policy Wording.pdf')
                && contentRaw.indexOf('Certificate of Currency.pdf')
                && contentRaw.indexOf('Tax Invoice and Policy Schedule.pdf'))
                return true;
        }

        return true;
    }

    depositAssureConciergeEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        //filter message via title
        if (messageObj.subject != 'Thank you for contacting Deposit Assure’s Concierge team.')
            return false;

        if (dateTimeNow > Date.parse(messageObj.created_at))
            return false;

        //filter message body
        if (messageObj.bodyText) {
            if (data.expectedEmailValues.emailSourceHashCode == messageObj.html_body_size
                && data.expectedEmailValues.attachmentSize == messageObj.human_size
                && messageObj.bodyText.indexOf("The team will prepare the deposit bond application")
                && messageObj.bodyText.indexOf("The application will be emailed to all applicants for signing with Docusign")
                && messageObj.bodyText.indexOf("Once the application is signed, we can have an approval within 1 business hour")
                && messageObj.bodyText.indexOf("Applicants will receive a link to pay for the deposit bond fee by credit card or debit card")) {
                return true;
            }
        }

        return true;
    }

    leaseAssetFinanceMotorpacEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        return true;
    }

    mgaPetsEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        //filter messages
        if (messageObj.subject != 'Your application was successfully submitted')
            return false;
        if (dateTimeNow > Date.parse(messageObj.created_at))
            return false;

        //filter message body
        if (messageObj.bodyText) {
            if (data.expectedEmailValues.emailSourceHashCode == messageObj.html_body_size
                && data.expectedEmailValues.attachmentSize == messageObj.human_size
                && messageObj.bodyText.indexOf("Your online insurance application has been completed.")
                && messageObj.bodyText.indexOf('Please note that the terms and conditions for the pay-by-the-month are attached to this email')
                //check attachments
                && contentRaw.indexOf('Pay-by-the-month terms and conditions.pdf')
                && contentRaw.indexOf('Policy Wording.pdf')
                && contentRaw.indexOf('Financial Services Guide.pdf'))
                return true;
        }

        return true;
    }

    insureMyPromoPromoInABoxEmailCheck(data, messageObj, contentRaw, dateTimeNow = null) {
        if (messageObj.subject != 'Your Promo in a Box Invoice')
            return false;
        if (dateTimeNow > Date.parse(messageObj.created_at))
            return false;

        //filter message body
        if (messageObj.bodyText) {
            if (data.expectedEmailValues.emailSourceHashCode == messageObj.html_body_size
                && data.expectedEmailValues.attachmentSize == messageObj.human_size
                && messageObj.bodyText.indexOf("Please find attached your invoice from Promo in a Box.")
                //check attachments   
                && contentRaw.indexOf('tax-invoice.pdf'))
                return true;
        }

        return true;
    }

    searchMessageAsync(data: any, searchFilter: (message: any) => boolean): Promise<Array<any>> {

        let _this = this;
        return new Promise(function (resolve, reject) {
            let lastId = 0;

            //for future reference for paginating messages on the inbox
            // let chain = new Promise<any>(function (resolve, reject) { resolve({ array: [], lastId: 0 }) });// _this.GetMessages(mailtrapInboxId, _this);
            // let filteredMessages = [];
            // while (lastId != null) {
            // chain = chain.then((o) => {
            //     lastId = o.lastId;
            //     if (o.array.length) {
            //         o.array.forEach(message => {
            //             var matched = searchFilter(message);
            //             if (matched) {
            //                 filteredMessages.push(message);
            //             }
            //         });
            //     }
            //     if (lastId != null) {
            //         return _this.GetMessages(mailtrapInboxId, _this);
            //     }
            // });
            // }

            _this.getMessagesAsync(data, _this, searchFilter).then(o => {
                lastId = o.lastId;
                if (o.array.length && lastId) {
                    resolve(o.array);
                }
                else {
                    resolve([]);
                }
            }).catch(err => {
                reject(err);
            });
        });
    }

    getMessagesAsync(data: any, _this, filterMessage): Promise<any> {

        let apiEndpoint = _this.endpoint + "/api/v1/inboxes/" + data.mailtrap.inboxId + "/messages";
        let _data = data;
        return new Promise(function (resolve, reject) {
            request({
                uri: apiEndpoint,
                method: 'GET',
                rejectUnauthorized: false,
                headers: {
                    'Authorization': 'Bearer ' + _this.token
                },
            },
                function (err, resp, body) {
                    if (err) {
                        reject();
                    } else {
                        (async () => {
                            let array = JSON.parse(body);
                            let filteredMessages = [];
                            for (let i = 0; i < array.length; i++) {
                                var message = array[i];
                                var filterPass = filterMessage(_data, message, null, _this.dateTimeNow);

                                if (filterPass) {
                                    var fileContent = await _this.getFile(message.txt_path, _this).then((body) => {
                                        return body;
                                    });
                                    var htmlSource = await _this.getFile(message.html_source_path, _this).then((body) => {
                                        return body;
                                    });

                                    var rawContent = await _this.getFile(message.raw_path, _this).then((body) => {
                                        return body;
                                    });
                                    message.bodyText = fileContent;
                                    console.log("File retrieved");

                                    if (filterMessage(_data, message, rawContent, _this.dateTimeNow)) {
                                        filteredMessages.push(message);
                                        break;
                                    }
                                }
                            }
                            let id = "";
                            if (array[array.length - 1])
                                id = array[array.length - 1].id;
                            resolve({ array: filteredMessages, lastId: id || null });

                        })();
                    }
                });
        });
    }

    getFile(path, _this) {
        let apiEndpoint = _this.endpoint + path;

        return new Promise(function (resolve, reject) {
            request({
                uri: apiEndpoint,
                method: 'GET',
                rejectUnauthorized: false,
                headers: {
                    'Authorization': 'Bearer ' + _this.token
                },
            },
                function (err, resp, body) {
                    if (err) {
                        reject();
                    } else {
                        resolve(body);
                    }
                });
        });
    }
}