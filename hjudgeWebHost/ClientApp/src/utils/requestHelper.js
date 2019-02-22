"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function ReadCookie(name) {
    name += '=';
    for (var ca = document.cookie.split(/;\s*/), i = ca.length - 1; i >= 0; i--)
        if (!ca[i].indexOf(name))
            return ca[i].replace(name, '');
    return '';
}
exports.ReadCookie = ReadCookie;
function Post(url, data) {
    if (data === void 0) { data = {}; }
    var token = ReadCookie('XSRF-TOKEN');
    var myInit = {
        method: 'POST',
        credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json',
            'X-XSRF-TOKEN': token
        },
        body: JSON.stringify(data),
        mode: 'cors',
        cache: 'default'
    };
    var myRequest = new Request(url);
    return fetch(myRequest, myInit);
}
exports.Post = Post;
function Get(url, data) {
    if (data === void 0) { data = {}; }
    var paramStr = '?';
    for (var x in data) {
        paramStr += x + "=" + escape(data[x]) + "&";
    }
    paramStr = paramStr.substring(0, paramStr.length - 1);
    var myInit = {
        method: 'GET',
        credentials: 'same-origin',
        mode: 'cors',
        cache: 'default'
    };
    var myRequest = new Request(url + paramStr);
    return fetch(myRequest, myInit);
}
exports.Get = Get;
//# sourceMappingURL=requestHelper.js.map