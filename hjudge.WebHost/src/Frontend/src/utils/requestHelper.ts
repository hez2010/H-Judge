import fetchPonyfill from 'fetch-ponyfill';
const { fetch, Request } = fetchPonyfill();

export function ReadCookie(name: string) {
  name += '=';
  for (let ca = document.cookie.split(/;\s*/), i = ca.length - 1; i >= 0; i--)
    if (!ca[i].indexOf(name))
      return ca[i].replace(name, '');
  return '';
}

export function Post(url: string, data: any = {}, json: boolean = true, contentType: string = 'application/json') {
  let token = ReadCookie('XSRF-TOKEN');

  let myInit: RequestInit = {
    method: 'POST',
    credentials: 'same-origin',
    headers: !!contentType ? {
      'Content-Type': contentType,
      'X-XSRF-TOKEN': token
    } : {
        'X-XSRF-TOKEN': token
      },
    body: json ? JSON.stringify(data) : data,
    mode: 'cors',
    cache: 'default'
  };

  let myRequest = new Request(url);

  return fetch(myRequest, myInit);
}

export function Put(url: string, data: any = {}, json: boolean = true, contentType: string = 'application/json') {
  let token = ReadCookie('XSRF-TOKEN');

  let myInit: RequestInit = {
    method: 'PUT',
    credentials: 'same-origin',
    headers: !!contentType ? {
      'Content-Type': contentType,
      'X-XSRF-TOKEN': token
    } : {
        'X-XSRF-TOKEN': token
      },
    body: json ? JSON.stringify(data) : data,
    mode: 'cors',
    cache: 'default'
  };

  let myRequest = new Request(url);

  return fetch(myRequest, myInit);
}

export function Get(url: string, data: any = {}) {
  let token = ReadCookie('XSRF-TOKEN');

  let paramStr = '?';
  for (let x in data) {
    paramStr += `${x}=${escape(data[x])}&`;
  }

  paramStr = paramStr.substring(0, paramStr.length - 1);

  let myInit: RequestInit = {
    method: 'GET',
    credentials: 'same-origin',
    headers: {
      'X-XSRF-TOKEN': token
    },
    mode: 'cors',
    cache: 'default'
  };

  let myRequest = new Request(url + paramStr);

  return fetch(myRequest, myInit);
}

export function Delete(url: string, data: any = {}) {
  let token = ReadCookie('XSRF-TOKEN');

  let paramStr = '?';
  for (let x in data) {
    paramStr += `${x}=${escape(data[x])}&`;
  }

  paramStr = paramStr.substring(0, paramStr.length - 1);

  let myInit: RequestInit = {
    method: 'DELETE',
    credentials: 'same-origin',
    headers: {
      'X-XSRF-TOKEN': token
    },
    mode: 'cors',
    cache: 'default'
  };

  let myRequest = new Request(url + paramStr);

  return fetch(myRequest, myInit);
}
