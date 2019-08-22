import * as React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import { setGlobal } from 'reactn';
import { UserInfo } from './interfaces/userInfo';
import { GlobalState } from './interfaces/globalState';

PromisePolyfill.polyfill();

let Global = global as any;

const getInitUserInfo = (userInfo?: UserInfo) => userInfo ? userInfo : {
  userId: '',
  userName: '',
  privilege: 4,
  name: '',
  email: '',
  signedIn: false,
  emailConfirmed: false,
  coins: 0,
  experience: 0,
  otherInfo: [],
  phoneNumber: '',
  phoneNumberConfirmed: false
};

Global.React = React;
Global.ReactDOM = ReactDOM;
Global.ReactDOMServer = ReactDOMServer;
Global.AppComponent = (props: any) => {
  setGlobal<GlobalState>({ userInfo: getInitUserInfo(props ? props.userInfo : undefined) });
  return <App />;
}

if (typeof window !== 'undefined') {
  setGlobal<GlobalState>({ userInfo: getInitUserInfo() });
  ReactDOM.render(<App />, document.querySelector('main'));
}