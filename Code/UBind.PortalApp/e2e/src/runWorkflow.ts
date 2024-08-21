import { AppPage } from './app.po';
import { browser, protractor } from 'protractor';
import { MenuAppPage } from './pageobjects/menu/menu.app.po';
import { SeedService } from './services/seed.service';
import { LoginAppPage } from './pageobjects/login/login.app.po';
import { TenantAppPage } from './pageobjects/tenant/tenant.app.po';
import { TestDataService } from './services/testdata.service';
import { ProductAppPage } from './pageobjects/product/product.app.po';
import { ReleaseAppPage } from './pageobjects/release/release.app.po';
import { QuoterAppPage } from '../../../webFormApp/e2e/app.quoter.po';
import { MailTrapService } from './services/mailtrap.service';
import { CustomerAppPage } from './pageobjects/customer/customer.app.po';
import { QuoteAppPage } from './pageobjects/quote/quote.app.po';
import { PolicyAppPage } from './pageobjects/policy/policy.app.po';
import { EmailAppPage } from './pageobjects/email/email.app.po';

let errorMessages = [];
let sampleData = null;
var desktopWidth = 1355;
var desktopHeight = 1060;
var viewPortType = "desktop";

export function runWorkflow(sampleDataParam = null) {
    describe('Workflow Test', () => {
        let proceedCheck: boolean = true;
        let page: AppPage;
        let loginPage: LoginAppPage;


        beforeEach(async () => {
            if (sampleDataParam != null)
                sampleData = sampleDataParam;
            if (!sampleData)
                sampleData = new TestDataService().retrieveAll(browser.collection)[0];
            page = new AppPage();
            loginPage = new LoginAppPage();
            jasmine.DEFAULT_TIMEOUT_INTERVAL = 900000;
            page.setBaseUrl(sampleData.endpoint.portal);
            await browser.getProcessedConfig().then(async x => {
                if (x.capabilities.chromeOptions.mobileEmulation) {
                    browser.viewPortType = viewPortType = "mobile";
                } else {
                    browser.viewPortType = viewPortType = "desktop";
                    // // // //set default size
                    await browser.driver.manage().window().setSize(desktopWidth, desktopHeight);
                }
            });

            await browser.waitForAngularEnabled(true);
        });

        describe('Tenant: Creation Process/', () => {

            beforeEach(() => {
                page.sleepInSeconds(2);
            });

            it('1.) Login', () => {
                log(sampleData.tenantId, sampleData.productId, viewPortType);
                //i need to navigate twice because the api on the portal is having problem
                // loginPage.navigate(sampleData.ubindLogin.adminTenantId, sampleData.environment);
                // page.sleepInSeconds(10);
                loginPage.navigate(sampleData.ubindLogin.adminTenantId, sampleData.environment).then(x => {
                    return loginPage.fillLoginPage(sampleData.ubindLogin.adminUsername, sampleData.ubindLogin.adminPassword).then(x => {
                        return loginPage.submit();
                    });
                }).catch(e => {
                    console.log(e);
                    errorMessages.push(e);
                    throw JSON.stringify(e);
                });
            });

            describe("2.) Process", () => {
                let menuPage: MenuAppPage;
                let tenantPage: TenantAppPage;

                beforeEach(() => {
                    menuPage = new MenuAppPage();
                    tenantPage = new TenantAppPage();
                });

                it('2.1) Navigation', () => {
                    menuPage.navigateToTenant().catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.2) Create A Tenant', () => {
                    tenantPage.checkTenantListIfExisting(sampleData.tenantName).then(exists => {
                        if (exists == false) {
                            return tenantPage.clickListTenantAddIcon().then(x => {
                                return tenantPage.fillAddTenantModal(sampleData.tenantName, sampleData.tenantId).then(x => {
                                    tenantPage.saveNewTenant();
                                    tenantPage.ignoreAlert();
                                    return tenantPage.ignoreAlert();
                                });
                            });
                        }
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.2) Activate Settings', () => {
                    tenantPage.clickList(sampleData.tenantName).then(x => {
                        return tenantPage.clickDetailSegment("Settings").then(x => {
                            return tenantPage.toggleAllSettings().then(x => {
                                if (viewPortType == "mobile") {
                                    return tenantPage.clickBackButton().then(x => {
                                    }).catch(x => {
                                    });
                                }
                            });
                        });
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.3) Logout', () => {
                    if (viewPortType == "mobile") {
                        tenantPage.clickBackButton().then(x => {
                        }).catch(x => {
                        });
                    }

                    menuPage.navigateToSignout().then(x => {
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });
            });
        });

        describe('Product: Creation Process/', () => {
            let menuPage: MenuAppPage;
            let tenantPage: TenantAppPage;

            beforeEach(() => {
                page.sleepInSeconds(2);
                menuPage = new MenuAppPage();
                tenantPage = new TenantAppPage();
            });

            it('1.) Login', () => {
                log(sampleData.tenantId, sampleData.productId, viewPortType);
                loginPage.navigate(sampleData.ubindLogin.adminTenantId, sampleData.environment).then(x => {
                    return loginPage.fillLoginPage(sampleData.ubindLogin.adminUsername, sampleData.ubindLogin.adminPassword).then(x => {
                        return loginPage.submit();
                    });
                }).catch(e => {
                    console.log(e);
                    errorMessages.push(e);
                    throw JSON.stringify(e);
                });
            });

            describe("2.) Process", () => {

                beforeEach(() => {
                });

                it('2.1) Navigation', () => {
                    menuPage.navigateToTenant().catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.2) Create Product From Tenant', () => {
                    tenantPage.clickList(sampleData.tenantName).then(x => {
                        return tenantPage.clickDetailSegment("Products").then(x => {
                            return tenantPage.checkProductListIfExisting(sampleData.productName, sampleData.productId).then(exists => {
                                if (exists == false) {
                                    return tenantPage.clickAddProductIcon().then(x => {
                                        return tenantPage.fillAddProductModal(sampleData.productName, sampleData.productId).then(x => {
                                            return tenantPage.saveNewProduct().then(x => {
                                                tenantPage.ignoreAlert().catch(x => {
                                                });
                                                tenantPage.ignoreAlert().catch(x => {
                                                });
                                                //add delay just after creating the product. because of background processes.
                                                return page.sleepInSeconds(20).then(x => {
                                                    if (viewPortType == "mobile") {
                                                        return tenantPage.clickBackButton().catch(x => {
                                                        });
                                                    }
                                                });
                                            });
                                        });
                                    });
                                }
                                else if (viewPortType == "mobile") {
                                    tenantPage.clickBackButton().catch(x => {
                                    });
                                }
                            });
                        });
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.3) Logout', () => {
                    if (viewPortType == "mobile")
                        tenantPage.clickBackButton().catch(x => {
                        });
                    menuPage.navigateToSignout().then(x => {
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });
            });
        });

        describe("UploadSeedFiles/", () => {
            let seedService: SeedService;

            beforeEach(() => {
                seedService = new SeedService(sampleData.endpoint.api, sampleData.tenantId, sampleData.productId, sampleData.environment);
            });

            it('Upload Files', () => {
                try {
                    log(sampleData.tenantId, sampleData.productId, viewPortType);

                    var files = seedService.getFiles(sampleData.seed.directoryPath);
                    var uploads = seedService.uploadFiles(sampleData.ubindLogin.adminUsername, sampleData.ubindLogin.adminPassword, files);

                    uploads.then(results => {
                        page.sleepInSeconds(5);
                    }).catch(x => {
                        expect(x).toBeNull();
                    });

                    //waits for all the files to finish
                    page.sleepInSeconds(files.length * 2);
                } catch (e) {
                    console.log(e);
                    errorMessages.push(e);
                    throw JSON.stringify(e);
                }
            });
        });

        describe('Release Creation Process/', () => {

            beforeEach(() => {
                page.sleepInSeconds(2);
            });

            it('1.) Login', () => {
                log(sampleData.tenantId, sampleData.productId, viewPortType);
                loginPage.navigate(sampleData.ubindLogin.adminTenantId, sampleData.environment).then(x => {
                    return loginPage.fillLoginPage(sampleData.ubindLogin.adminUsername, sampleData.ubindLogin.adminPassword).then(x => {
                        return loginPage.submit();
                    });
                }).catch(e => {
                    console.log(e);
                    errorMessages.push(e);
                    throw JSON.stringify(e);
                });
            });

            describe("2.) Process", () => {
                let menuPage: MenuAppPage;
                let productPage: ProductAppPage;
                let releasePage: ReleaseAppPage;

                beforeEach(() => {
                    menuPage = new MenuAppPage();
                    productPage = new ProductAppPage();
                    releasePage = new ReleaseAppPage();
                });

                it('2.1) Navigation', () => {
                    menuPage.navigateToProducts().catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.2) Create Product Release', () => {
                    productPage.clickList(sampleData.productName).then(x => {
                        return productPage.clickDetailSegment("Releases").then(x => {
                            return productPage.clickAddReleaseIcon().then(x => {
                                return productPage.fillCreateReleaseModal(sampleData.releaseType, sampleData.releaseDescription).then(x => {
                                    return productPage.clickCreateReleaseSaveButton();
                                });
                            });
                        });
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });

                it('2.3) Promote Release to Staging', () => {
                    if (viewPortType == "desktop") {
                        releasePage.clickDetailMoreIcon().then(x => {
                            return releasePage.clickDetailMoreIconPopupList('Promote to staging');
                        }).catch(e => {
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    }
                    else if (viewPortType == "mobile") {
                        releasePage.clickListMostRecentRecord().then(x => {
                            return releasePage.clickDetailMoreIcon().then(x => {
                                return releasePage.clickDetailMoreIconPopupList('Promote to staging');
                            }).catch(e => {
                                browser.ignoreSynchronization = true;
                                releasePage.clickBackButton().then(x => {
                                    browser.ignoreSynchronization = false;
                                }).catch(x => {
                                });
                                throw e;
                            });
                        }).catch(e => {
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    }
                });

                it('2.4) Logout', () => {
                    menuPage.navigateToSignout().then(x => {
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                });
            });
        });

        describe("UploadClaimInvoicePolicyNumbers/", () => {
            let seedService: SeedService;

            beforeEach(() => {
                seedService = new SeedService(sampleData.endpoint.api, sampleData.tenantId, sampleData.productId, sampleData.environment);
            });

            var chain = function () {
                var defer = protractor.promise.defer();
                defer.fulfill(true);
                return defer.promise;
            };

            it('Upload Claim/Invoice/Policy Numbers', (done) => {
                try {
                    var promises: Promise<{}>[] = [];
                    promises.push(seedService.seedClaimNumbers());
                    promises.push(seedService.seedPolicyNumbers());
                    promises.push(seedService.seedInvoiceNumbers());

                    chain()
                        .then(function () {
                            // Save data
                        }).then(function () {
                            Promise.all(promises).then(x => {
                                expect(x.length).toBeGreaterThan(0, "upload successful");
                                done();
                            }).catch(x => {
                                expect(x).toBeNull();
                                done();
                            });
                        });
                } catch (e) {
                    console.log(e);
                    errorMessages.push(e);
                    throw JSON.stringify(e);
                }
            });
        });

        describe('', () => {
            let continueEmailVerification = true;

            describe('UBIND Quoter App/', () => {
                let page: QuoterAppPage;

                let frame: string = 'ubindProduct0';

                beforeEach(() => {
                    page = new QuoterAppPage(frame);
                    browser.waitForAngularEnabled(false);
                    page.setBaseUrl(sampleData.endpoint.quoter);
                    page.navigateTo('/assets/landing-page.html?tenant=' + sampleData.tenantId + '&product=' + sampleData.productId + '&environment=' + sampleData.environment + '');
                    page.sleepInSeconds(5);
                });

                it('Fillup Quoter App ', () => {
                    log(sampleData.tenantId, sampleData.productId, viewPortType);

                    // // // // wait for it to load some more
                    page.sleepInSeconds(10).then(sleep => {
                        let p = Promise.resolve(true);
                        return p.then(function () {
                            return page.fillupForm(sampleData.quoter.model).then(x => {
                                page.sleepInSeconds(60);
                                continueEmailVerification = true;
                            });
                        })
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        continueEmailVerification = false;
                        throw JSON.stringify(e);
                    });
                });

                afterEach(() => {
                    page.setBaseUrl(sampleData.endpoint.portal);
                });
            });

            describe('Quote Email Verification/', () => {
                let mailTrapService: MailTrapService;

                beforeEach(() => {
                    mailTrapService = new MailTrapService(sampleData.mailtrap.apiToken, Date.now());
                });

                var chain = function () {
                    var defer = protractor.promise.defer();
                    defer.fulfill(true);
                    return defer.promise;
                };

                it('Verify Email Sent', (done) => {
                    if (continueEmailVerification && sampleData.checkResults.email) {
                        //filter messages
                        var searchFilter = mailTrapService[sampleData.mailtrap.emailCheckFunction];

                        chain()
                            .then(function () { })
                            .then(async function () {
                                let continueLoop = true;
                                let cycle = 15;
                                let seconds = 10;
                                //retieve emails on mailtrap
                                //call this every 10seconds until the timeout happens
                                while (continueLoop && cycle > 0) {
                                    await page.sleepInSeconds(seconds);
                                    await mailTrapService.searchMessageAsync(sampleData, searchFilter).then(filteredMessages => {
                                        if (filteredMessages.length > 0) {
                                            continueLoop = false;
                                            done();
                                            expect(filteredMessages.length).toBeGreaterThan(0);
                                            return proceedCheck = true;
                                        } else {
                                            console.log("there is no message recieved on this cycle yet. will retry in " + seconds + " sec");
                                            cycle -= 1;
                                        }
                                    }).catch(e => {
                                        proceedCheck = false;
                                        console.log(e);
                                        errorMessages.push(e);
                                        throw JSON.stringify(e);
                                    });
                                }
                                if (cycle <= 0) {
                                    proceedCheck = false;
                                    //cycle finishes but there is no message recieved
                                    expect(cycle).toBeGreaterThan(0, "cycle finished without finding email");
                                    done();
                                }
                            });
                    } else {
                        console.log("skipped");
                        proceedCheck = false;
                        done();
                    }
                });
            });
        });

        describe('Check Client Admin For Data Created/', () => {
            let customerPage: CustomerAppPage;
            let menuPage: MenuAppPage;
            let quotePage: QuoteAppPage;
            let policyPage: PolicyAppPage;
            let emailPage: EmailAppPage;
            let hasCheckResult = false;
            beforeEach(() => {
                menuPage = new MenuAppPage();
                customerPage = new CustomerAppPage();
                quotePage = new QuoteAppPage();
                emailPage = new EmailAppPage();
                page.sleepInSeconds(1.5);
                //emailRetrieved = true;
                if (sampleData.checkResults != null) {
                    if (sampleData.checkResults.email ||
                        sampleData.checkResults.customer ||
                        sampleData.checkResults.policy ||
                        sampleData.checkResults.quote) {
                        hasCheckResult = true;
                    }
                    else {
                        hasCheckResult = false;
                    }
                } else {
                    hasCheckResult = true;
                }
            });

            it('Login', () => {
                browser.waitForAngularEnabled(true);
                if (proceedCheck && hasCheckResult) {
                    log(sampleData.tenantId, sampleData.productId, viewPortType);
                    loginPage.navigate(sampleData.ubindLogin.clientAdminTenantId, sampleData.environment);
                    page.sleepInSeconds(7);
                    loginPage.navigate(sampleData.ubindLogin.clientAdminTenantId, sampleData.environment).then(x => {
                        return loginPage.fillLoginPage(sampleData.ubindLogin.clientAdminUsername, sampleData.ubindLogin.clientAdminPassword).then(x => {
                            return loginPage.submit().then(x => {
                                return page.sleepInSeconds(2.5);
                            });
                        });
                    }).catch(e => {
                        proceedCheck = false;
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                } else {
                    console.log("skipped");
                }
            });

            describe("Process/", () => {

                it('Check Customer List/', (done) => {
                    if (proceedCheck && sampleData.checkResults.customer) {
                        customerPage = new CustomerAppPage();
                        menuPage.navigateToCustomers().then(x => {
                            return customerPage.clickNewListSegment().then(x => {
                                return customerPage.checkListIfExists(sampleData.contactName).then(exists => {
                                    expect(exists).toBe(true, "customer does not exist");
                                    done();
                                });
                            });
                        }).catch(e => {
                            e.param = sampleData.contactName;
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    } else {
                        console.log("skipped");
                        done();
                    }
                });

                it('Check Quote List/', (done) => {
                    if (proceedCheck && sampleData.checkResults.quote) {
                        quotePage = new QuoteAppPage();
                        menuPage.navigateToQuotes().then(x => {
                            return quotePage.clickCompleteListSegment().then(x => {
                                return quotePage.checkListIfExists(sampleData.contactName).then(exists => {
                                    expect(exists).toBe(true, "quote does not exist");
                                    done();
                                });
                            });
                        }).catch(e => {
                            e.param = sampleData.contactName;
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    } else {
                        console.log("skipped");
                        done();
                    }
                });

                it('Check Policy List/', (done) => {
                    if (proceedCheck && sampleData.checkResults.policy) {
                        policyPage = new PolicyAppPage();
                        menuPage.navigateToPolicies().then(async x => {
                            return policyPage.hasPolicy(sampleData.contactName).then(hasPolicy => {
                                expect(hasPolicy).toBe(true, "policy does not exist");
                                done();
                            });
                        }).catch(e => {
                            e.param = sampleData.contactName;
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    } else {
                        console.log("skipped");
                        done();
                    }
                });

                it('Check Export Policy/', (done) => {
                    if (proceedCheck && sampleData.checkResults.policy) {
                        policyPage = new PolicyAppPage();
                        menuPage.navigateToPolicies().then(async x => {
                            return policyPage.checkExport(sampleData).then(exportWorking => {
                                expect(exportWorking).toBe(true, "export failed!");
                                done();
                            });
                        }).catch(e => {
                            console.log(e);
                            errorMessages.push(e);
                            throw JSON.stringify(e);
                        });
                    } else {
                        console.log("skipped");
                        done();
                    }
                });

                it('Check Email List/', (done) => {
                    if (proceedCheck && sampleData.checkResults.email) {
                        emailPage = new EmailAppPage();

                        page.retryPromise(1,
                            (resolve, reject) => {
                                return menuPage.navigateToEmails().then(x => {
                                    return emailPage.clickAdminListSegment().then(x => {
                                        return emailPage.checkListIfExists(sampleData.contactEmail).then(exists => {
                                            expect(exists).toBe(true, "email does not exist");
                                            resolve();
                                            done();
                                        });
                                    });
                                }).catch(e => {
                                    reject(e);
                                });
                            }, 1).catch(e => {
                                e.param = sampleData.contactEmail;
                                console.log(e);
                                errorMessages.push(e);
                                throw JSON.stringify(e);
                            });
                    } else {
                        console.log("skipped");
                        done();
                    }
                });
            });

            it('Logout/', () => {
                if (proceedCheck && hasCheckResult) {
                    menuPage.navigateToSignout().then(x => {
                        return page.sleepInSeconds(5);
                    }).catch(e => {
                        console.log(e);
                        errorMessages.push(e);
                        throw JSON.stringify(e);
                    });
                } else {
                    console.log("skipped");
                }
            });
        });
    });
}


function log(tenantId, productId, viewPort) {
    console.log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
    console.log("Viewport : " + viewPort);
    console.log("Tenant: " + tenantId + " Product: " + productId);
    console.log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
}
