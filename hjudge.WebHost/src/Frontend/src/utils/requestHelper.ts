import fetchPonyfill from 'fetch-ponyfill';
const { fetch, Request, Response, Headers } = fetchPonyfill();

export function ReadCookie(name: string) {
  name += '=';
  for (let ca = document.cookie.split(/;\s*/), i = ca.length - 1; i >= 0; i--)
    if (!ca[i].indexOf(name))
      return ca[i].replace(name, '');
  return '';
}

export function Post(url: string, data: any = {}) {
  let token = ReadCookie('XSRF-TOKEN');

  let myInit: RequestInit = {
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

  let myRequest = new Request(url);

  return fetch(myRequest, myInit);
}

export function Get(url: string, data: any = {}) {
  let paramStr = '?';
  for (let x in data) {
    paramStr += `${x}=${escape(data[x])}&`;
  }

  paramStr = paramStr.substring(0, paramStr.length - 1);

  let myInit: RequestInit = {
    method: 'GET',
    credentials: 'same-origin',
    mode: 'cors',
    cache: 'default'
  };

  let myRequest = new Request(url + paramStr);

  return fetch(myRequest, myInit);
}
