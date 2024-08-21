import { browser, by, element, ProtractorBy, Button, ElementFinder, protractor } from 'protractor';
import { AppPage } from './app.po';
var path = require('path');

export class QuoterAppPage extends AppPage {

  public mainFrame: string = null;

  constructor(mainFrame: string) {
    super();
    this.mainFrame = mainFrame;
  }

  public fillupForm(model: any, fromIframe: boolean = false) {
    var array = [];
    for (var property in model) {
      if (model.hasOwnProperty(property)) {
        var value = model[property];
        array.push({ "property": property, "value": value });
      }
    }

    if (fromIframe == false) {
      return this.backToFrame(this.mainFrame).then((x) => {
        // throw "something";
      })
        .catch((x) => {
          // when error. try again.
          browser.refresh();
          this.sleepInSeconds(20);
          return this.backToFrame(this.mainFrame).then((x) => {
            return this.process(array);
          });
          // throw x;
        }).then((x) => {
          return this.process(array);
        });
    } else {
      return this.processIframe(array).then((x) => {
        return this.backToFrame(this.mainFrame);
      });
    }
  }

  private async process(array: any): Promise<any> {
    let itemNumber: number = 0;
    let waitTimeSeconds: number = 0;
    let _this = this;

    new Promise(function (resolve, reject) {
      array.forEach((item) => {
        itemNumber++;
        var field: string = item.property;
        var value: string = item.value;
        if (field.startsWith('action')) {
          if (value == "next") {
            return _this.footerClickNext(300);
          }
          if (value == "back") {
            return _this.FooterClickBack(300);
          }
        } else if (field.startsWith('delay')) {
          return _this.sleepInSeconds(value);
        } else {
          // wait for the first element longer
          waitTimeSeconds = itemNumber == 1 ? 300 : 60;
          var anchorElementId = _this.appendPrefixIfDoesntExist("anchor-", field);
          var elementId = element(by.id(anchorElementId));

          return (_this.waitForElementVisible(elementId, field, waitTimeSeconds).then(async (x) => {
            var innerField = await _this.getTagName(elementId).then((innerField) => {
              return innerField;
            });

            switch (innerField) {
              case "checkbox-field":
              case "toggle-field":
                if (value) {
                  return _this.checkboxClick(anchorElementId, waitTimeSeconds);
                }
                break;
              case "select-field":
              case "search-select-field":
              case "textarea-field":
              case "datepicker-field":
              case "input-field":
                return _this.processSendKeys(anchorElementId, value, waitTimeSeconds);
              case "buttons-field":
                return _this.buttonClick(anchorElementId, value, waitTimeSeconds);
              case "radio-field":
                return _this.radioSelect(anchorElementId, value, waitTimeSeconds);
              case "attachment-field":
                return _this.attachmentUpload(anchorElementId, value, waitTimeSeconds);
              case "iframe-field":
                var frameResult = _this.iframeProcess(anchorElementId, value, 'iframe-field', waitTimeSeconds)
                return frameResult;
              default:
                break;
            }
          }));
        }
      });
    });
  }

  private async processIframe(array: any): Promise<any> {
    var i: number = 0;
    var _this = this;
    return new Promise(function (resolve, reject) {
      array.forEach(async (item) => {
        var field: string = item.property;
        var value: string = item.value;

        var el = element(by.id(field));

        _this.waitForElementVisible(el, field);

        var className = await el.getAttribute("class").then((className) => {
          return className;
        });

        switch (className) {
          // for iframes
          case "carousel-inner":
            _this.childClick(field, value);
            break;
          default:
            break;
        }

        i++;

        if (array.length == i) {
          // just added this to have a delay and finish all the other async calls first.
          browser.getPageSource().then((x) => {
            resolve(x);
          });
        }
      });
    });
  }
  public iframeProcess(anchorElementId: string, iframeObj: any, elementName:string,  waitTimeSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);

    var el = element(by.id(anchorElementId));
    this.waitForElementVisible(el, anchorElementId, waitTimeSeconds);
    var iframeFieldEl = el.all(by.tagName(elementName)).get(0);
    this.waitForElementVisible(iframeFieldEl, anchorElementId, waitTimeSeconds);
    var iframeEl = iframeFieldEl.all(by.tagName('iframe')).get(0);
    var _this = this;

    return this.focusToFrameByElement(iframeEl, anchorElementId).then((x) => {
      return _this.fillupForm(iframeObj, true).then((x) => {
      });
    });
  }
}