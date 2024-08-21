function findGetParameter(parameterName) {
    var result = null;
    var tmp = [];
    location.search
        .substr(1)
        .split("&")
        .forEach(function (item) {
            tmp = item.split("=");
            if (tmp[0] === parameterName) result = decodeURIComponent(tmp[1]);
        });
    return result;
}

var frameId = findGetParameter('frameId');
if (frameId && window.parent) {
    // tell the parent window we have started loading
    {
        try {
            let data = {
                'messageType': 'frameLoading',
                'frameId': frameId,
                'payload': null,
            };

            let postDomainSlashPosition = document.referrer.indexOf('/', 8);
            let originOfParent = postDomainSlashPosition == -1
                ? document.referrer
                : document.referrer.substring(0, postDomainSlashPosition);

            window.parent.postMessage(data, originOfParent);
        } catch (err) {
        }
    }
}