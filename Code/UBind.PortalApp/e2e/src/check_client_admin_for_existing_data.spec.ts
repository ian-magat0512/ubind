import { AppPage } from './app.po';
import { browser } from 'protractor';
import { MenuAppPage } from './pageobjects/menu/menu.app.po';
import { LoginAppPage } from './pageobjects/login/login.app.po';
import { TestDataService } from './services/testdata.service';
import { CustomerAppPage } from './pageobjects/customer/customer.app.po';
import { QuoteAppPage } from './pageobjects/quote/quote.app.po';
import { PolicyAppPage } from './pageobjects/policy/policy.app.po';
import { EmailAppPage } from './pageobjects/email/email.app.po';
import { QuoterAppPage } from '../../../webFormApp/e2e/app.quoter.po';

let sampleData = null;
let errorMessages = [];
runCheck();
export function runCheck(sampleDataParam = null) {

  describe('Check Client Admin For Data Created', () => {
    let loginPage: LoginAppPage;
    let page: AppPage;
    let menuPage: MenuAppPage;
    let emailRetrieved = true;

    beforeEach(() => {
      if (sampleDataParam)
        sampleData = sampleDataParam;
      if (!sampleData)
        sampleData = new TestDataService().retrieveAll(browser.collection)[5];
      loginPage = new LoginAppPage();
      page = new AppPage();
      menuPage = new MenuAppPage();
      jasmine.DEFAULT_TIMEOUT_INTERVAL = 1000000;
      page.setBaseUrl(sampleData.endpoint.portal);
      browser.waitForAngularEnabled(true);
    });

    describe('', () => {
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
          browser.waitForAngularEnabled(false);
          //wait for it to load some more
          page.sleepInSeconds(10).then(sleep => {
            let p = Promise.resolve(true);
            return p.then(function () {
              return page.fillupForm(sampleData.quoter.model).then(x => {
                page.sleepInSeconds(30);
                emailRetrieved = true;
              });
            })
          }).catch(e => {
            console.log(e);
            errorMessages.push(e);
            throw JSON.stringify(e);
          });
        });
      });
    });

    describe('Check Client Admin For Data Created/', () => {
      let customerPage: CustomerAppPage;
      let menuPage: MenuAppPage;
      let quotePage: QuoteAppPage;
      let policyPage: PolicyAppPage;
      let emailPage: EmailAppPage;
      let proceedCheck = false;
      beforeEach(() => {
        menuPage = new MenuAppPage();
        customerPage = new CustomerAppPage();
        quotePage = new QuoteAppPage();
        emailPage = new EmailAppPage();
        page.sleepInSeconds(1.5);

        if (sampleData.checkResults != null) {
          if (sampleData.checkResults.email ||
            sampleData.checkResults.customer ||
            sampleData.checkResults.policy ||
            sampleData.checkResults.quote) {
            proceedCheck = true;
          }
          else {
            proceedCheck = false;
          }
        } else {
          proceedCheck = true;
        }
      });

      it('Login', () => {
        if (!emailRetrieved || !proceedCheck) {
          console.log("skipped");
        } else {
          page.sleepInSeconds(5);
          loginPage.navigate(sampleData.ubindLogin.clientAdminTenantId, sampleData.environment).then(x => {
            return loginPage.fillLoginPage(sampleData.ubindLogin.clientAdminUsername, sampleData.ubindLogin.clientAdminPassword).then(x => {
              return loginPage.submit().then(x => {
                return page.sleepInSeconds(2.5);
              });
            });
          }).catch(e => {
            console.log(e);
            errorMessages.push(e);
            throw JSON.stringify(e);
          });
        }
      });

      describe("Process/", () => {

        it('Check Customer List/', (done) => {
          if (!emailRetrieved || !sampleData.checkResults.customer) {
            console.log("skipped");
            done();
          } else {
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
          }
        });

        it('Check Quote List/', (done) => {
          if (!emailRetrieved || !sampleData.checkResults.quote) {
            console.log("skipped");
            done();
          } else {
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
          }
        });

        it('Check Policy List/', (done) => {
          if (!emailRetrieved || !sampleData.checkResults.policy) {
            console.log("skipped");
            done();
          } else {
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
          }
        });

        it('Check Export Policy/', (done) => {
          if (!emailRetrieved || !sampleData.checkResults.policy) {
            console.log("skipped");
            done();
          } else {
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
          }
        });

        it('Check Email List/', (done) => {
          if (!emailRetrieved || !sampleData.checkResults.email) {
            console.log("skipped");
            done();
          }
          else {
            emailPage = new EmailAppPage();

            retryPromise(1,
              (resolve, reject) => {
                return menuPage.navigateToEmails().then(x => {
                  return emailPage.clickAdminListSegment().then(x => {
                    return emailPage.checkListIfExists(sampleData.contactEmail).then(exists => {
                      expect(exists).toBe(true, "email does not exist");
                      done();
                    });
                  });
                }).catch(e => {
                  reject(e);
                });
              }).catch(e => {
                e.param = sampleData.contactEmail;
                console.log(e);
                errorMessages.push(e);
                throw JSON.stringify(e);
              });
          }
        });
      });

      it('Logout/', () => {
        if (!emailRetrieved || !proceedCheck) {
          console.log("skipped");
        } else {
          menuPage.navigateToSignout().then(x => {
          }).catch(e => {
            console.log(e);
            errorMessages.push(e);
            throw JSON.stringify(e);
          });
        }
      });
    });
  });
}

function retryPromise(retries, executor) {
  if (typeof retries !== 'number') {
    throw new TypeError('retries is not a number')
  }

  return new Promise(executor).then(x => {

  }).catch(error => {
    if (retries > 0) {
      return retryPromise(retries - 1, executor);
    }
    else {
      Promise.reject(error);
      throw error;
    }
  });
}