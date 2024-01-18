'use strict';
var HSO = HSO || { namespace: true };
HSO.Account = HSO.Account || { namespace: true };

HSO.Account.Fields = {
    DisplayName: "name",
    BankNumber:"hso_bankaccount"
};

HSO.Account.Constants = {
    IBANApiKey: "820d953419e4c3bf9f84c753310ca480b2815fed",
};
HSO.Account.onLoad = function (executionContext) {
    "use strict";
};

HSO.Account.onFieldChange = function (executionContext) {
    "use strict";
};

HSO.Account.onSave = function (executionContext) {
    "use strict";
    try {
        var formContext = executionContext.getFormContext();
        var isbankNumberFieldChanged = formContext.getAttribute(HSO.Account.Fields.BankNumber) === null ? false : formContext.getAttribute(HSO.Account.Fields.BankNumber).getIsDirty();
        if (isbankNumberFieldChanged) {
            var bankNumber = formContext.getAttribute(HSO.Account.Fields.BankNumber) === null ? null : formContext.getAttribute(HSO.Account.Fields.BankNumber).getValue();
            debugger;
            var ibanApi = IBANApi();
            ibanApi.validateIBANBasic(bankNumber, HSO.Account.Constants.IBANApiKey).then(function (jsonoutputdata) {
                debugger;
                if (jsonoutputdata.result != 200) {
                    console.log(jsonoutputdata);
                    Xrm.Navigation.openAlertDialog({ text: "" + jsonoutputdata.message });
                    formContext.getAttribute(HSO.Account.Fields.BankNumber).setValue("");
                    executionContext.getEventArgs().preventDefault();
                }
                else {
                    console.log(jsonoutputdata);
                }

            });
        }
    }
    catch (error) {
        Xrm.Navigation.openAlertDialog({ text: "" + error.message });
    }

};