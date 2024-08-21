import { AppPage } from '@app/app.po';
import { PolicyPageModel } from './policy-page.model';
import { element, by, browser } from 'protractor';

export class PolicyAppPage extends AppPage {

    policyPageModel: PolicyPageModel;

    public constructor() {
        super();
        this.policyPageModel = new PolicyPageModel();
    }

    clickActiveListSegment() {
        return this.clickSegment(this.policyPageModel.ListPolicyIonSegmentId, "ACTIVE");
    }

    clickPendingListSegment() {
        return this.clickSegment(this.policyPageModel.ListPolicyIonSegmentId, "PENDING");
    }

    checkPendingListIfExists(value) {
        return this.clickIonList(this.policyPageModel.ListPolicyPendingIonListId, value, true, false, false);
    }

    checkActiveListIfExists(value) {
        return this.clickIonList(this.policyPageModel.ListPolicyActiveIonListId, value, true, false, false);
    }

    clickMoreIcon() {
        return element(by.id(this.policyPageModel.ListPolicyMoreIconId)).click();
    }

    async hasPolicy(contactName) {
        var activePolicyCheck = await this.clickActiveListSegment().then(x => {
            return this.checkActiveListIfExists(contactName).then(exists => {
                return exists;
            });
        }).catch(x => {
            return false;
        });

        var hasPolicy = false;
        if (activePolicyCheck) {
            hasPolicy = true;
        } else {
            //try on pending tab
            hasPolicy = await this.clickPendingListSegment().then(x => {
                return this.checkPendingListIfExists(contactName).then(exists => {
                    return exists;
                });
            }).catch(x => {
                return false;
            });
        }

        return hasPolicy;
    }

    async checkExport(sampleData) {
        let waitExportInSeconds = 40;
        var activeListExport = await this.clickActiveListSegment().then(x => {
            return this.checkActiveListIfExists(sampleData.contactName).then(exists => {
                if (exists)
                    return this.clickMoreIcon().then(x => {
                        return this.exportPolicies("json").then(x => {
                            return this.sleepInSeconds(waitExportInSeconds).then(x => {
                                return this.checkLatestExportFileIfContains(sampleData);
                            });
                        });
                    });
                else
                    return false;
            }).catch(e => {
                return false;
            });
        });

        var exportWorking = false;
        if (activeListExport) {
            exportWorking = true;
        } else {
            var pendingListExport = await this.clickPendingListSegment().then(x => {
                return this.checkPendingListIfExists(sampleData.contactName).then(exists => {
                    if (exists)
                        return this.clickMoreIcon().then(x => {
                            return this.exportPolicies("json").then(x => {
                                return this.sleepInSeconds(waitExportInSeconds).then(x => {
                                    return this.checkLatestExportFileIfContains(sampleData);
                                });
                            });
                        });
                    else
                        return false;
                });
            }).catch(e => {
                return false;
            });

            exportWorking = pendingListExport ? true : false;
        }

        return exportWorking;
    }

    private exportPolicies(exportType) {
        return this.clickIonList(this.policyPageModel.ListPolicyMoreIconPopoverListId, "Export policies", true, true).then(x => {
            return this.clickButton(this.policyPageModel.ExportOptionsAlertBox, exportType).then(x => {
                return this.clickButton(this.policyPageModel.ExportOptionsAlertBox, "export");
            })
        });
    }

    private checkLatestExportFileIfContains(sampleData) {
        return browser.driver.get('chrome://downloads/').then(x => {
            return browser.executeScript('return downloads.Manager.get().items_').then(items => {
                var item = items[0];
                var fs = require('fs');
                var fileContent = fs.readFileSync(item.filePath, { encoding: 'utf8' });
                var hasContent = false;
                var policyList = JSON.parse(fileContent);

                for (let i = 0; i < policyList.length; i++) {
                    var policy = policyList[i];

                    //filter
                    if (policy.customerFullName == sampleData.contactName &&
                        policy.productId == sampleData.productId &&
                        policy.tenantId == sampleData.tenantId) {
                        hasContent = this.compare(policy.latestCalculationResultJson, sampleData.quoter.model);
                        break;
                    }
                }

                return browser.navigate().back().then(x => {
                    return hasContent;
                });
            });
        });
    }

    private compare(policyJson, quoterAppModel) {
        let match = false;
        for (var key in quoterAppModel) {
            var val = quoterAppModel[key];

            //skip these elements
            if (key.indexOf('action') >= 0 ||
                key.indexOf('delay') >= 0 ||
                key.indexOf('creditCardNumber') >= 0 ||
                key.indexOf('creditCard') >= 0 ||
                key.toLowerCase().indexOf('upload') >= 0 ||
                key.toLowerCase().indexOf('payment') >= 0)
                continue;

            //test purposes only
            // if (key.indexOf('contactName') >= 0 ||
            //     key.indexOf('contactEmail') >= 0)
            //     continue;

            //skip objects
            if (typeof val === "object") continue;

            if (typeof val === "boolean") val = val.toString();

            var formattedVal = this.formatAmount(val);

            var searchStringFormatted = "\"" + key + "\": \"" + formattedVal + "\"";
            var searchStringFormattedRemoveTrailing = "\"" + key + "\": \"" + formattedVal.replace('.00', '') + "\"";
            var searchStringRegular = "\"" + key + "\": \"" + val + "\"";
            var searchStringRegularWOQuotes = "\"" + key + "\": " + val;
            var searchStringLowered = "\"" + key + "\": \"" + val!.toLowerCase() + "\"";
            var searchStringLoweredWOQuotes = "\"" + key + "\": " + val!.toLowerCase();
            if (policyJson.indexOf(searchStringFormatted) >= 0 ||
                policyJson.indexOf(searchStringFormattedRemoveTrailing) >= 0 ||
                policyJson.indexOf(searchStringRegular) >= 0 ||
                policyJson.indexOf(searchStringRegularWOQuotes) >= 0 ||
                policyJson.indexOf(searchStringLowered) >= 0 ||
                policyJson.indexOf(searchStringLoweredWOQuotes) >= 0) {
                console.log(key + " -> " + val + " passed!");
            } else {
                console.log(key + " -> " + val + " failed!");
                return false;
            }
        }
        return true;
    }

    private formatAmount(val) {
        var formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
        });
        val = formatter.format(val);
        return val;
    }
}
