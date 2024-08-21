import { AppPage } from './app.po';
import { browser, protractor } from 'protractor';
import { MenuAppPage } from './pageobjects/menu/menu.app.po';
import { SeedService } from './services/seed.service';
import { LoginAppPage } from './pageobjects/login/login.app.po';
import { TenantAppPage } from './pageobjects/tenant/tenant.app.po';
import { TestDataService } from './services/testdata.service';
import { ProductAppPage } from './pageobjects/product/product.app.po';
import { ReleaseAppPage } from './pageobjects/release/release.app.po';


let errorMessages = [];
let sampleData = null;
var desktopWidth = 1355;
var desktopHeight = 1060;
var viewPortType = "desktop";

describe('Portal', () => {
    let page: AppPage;
    let loginPage: LoginAppPage;

    beforeEach(() => {
        if (!sampleData)
            sampleData = new TestDataService().retrieveAll(browser.collection)[0];
        page = new AppPage();
        loginPage = new LoginAppPage();
        jasmine.DEFAULT_TIMEOUT_INTERVAL = 750000;
        page.setBaseUrl(sampleData.endpoint.portal);
        browser.getProcessedConfig().then(x => {
            if (x.capabilities.chromeOptions.mobileEmulation) {
                viewPortType = "mobile";
            } else {
                viewPortType = "desktop";
                //set default size
                browser.driver.manage().window().setSize(desktopWidth, desktopHeight);
            }
        });

        browser.waitForAngularEnabled(true);
    });

    describe('Tenant: Creation Process/', () => {

        beforeEach(() => {
            page.sleepInSeconds(2);
        });

        it('1.) Login', () => {
            //i need to navigate twice because the api on the portal is having problem
            loginPage.navigate(sampleData.ubindLogin.adminTenantId, sampleData.environment);
            page.sleepInSeconds(10);
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
                                            return page.sleepInSeconds(60).then(x => {
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
            page.sleepInSeconds(2);
        });

        it('Upload Files', () => {
            try {
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
            page.sleepInSeconds(2);
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
});
