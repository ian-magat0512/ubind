
import { QuoterAppPage } from './app.quoter.po';
import { browser, by, element, $, protractor } from 'protractor';

describe('UBIND Quoter App', () => {
  let page: QuoterAppPage;
  let tenantId = 'demos';
  let productId = 'latitude-motor-loan';
  let environment = 'development';
  let frame = 'ubindProduct0';

  beforeEach(() => {
    jasmine.DEFAULT_TIMEOUT_INTERVAL = 100000;
    page = new QuoterAppPage(frame);
    browser.driver.manage().window().maximize();
    browser.waitForAngularEnabled(false);
    page.navigateTo('/assets/landing-page.html?tenant=' + tenantId + '&productId=' + productId + '&environment=' + environment + '');
  });

  it('Fillup Quoter App', () => {

    //wait for it to load some more
    browser.driver.sleep(10000).then(sleep => {
      
      page.focusToFrameById(frame);

     page.demosLatitudeMotorLoanFillupForm();
    });

    browser.pause(5000);
  });
});
