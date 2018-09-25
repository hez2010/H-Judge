export function Post(url, data = {}) {
    var myInit = {
        method: 'POST',
        credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data),
        mode: 'cors',
        cache: 'default'
    };

    var myRequest = new Request(url);

    return fetch(myRequest, myInit);
}

export function Get(url, data = {}) {
    var paramStr = '?';
    for (var x in data) {
        paramStr += `${x}=${escape(data[x])}&`;
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
