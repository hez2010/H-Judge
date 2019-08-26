import * as React from 'reactn';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import "regenerator-runtime/runtime";
import { UserInfo } from './interfaces/userInfo';

PromisePolyfill.polyfill();

let Global = global as any;
Global.ReactDOM = ReactDOM;
Global.React = React;
Global.ReactDOMServer = ReactDOMServer;

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

if (typeof window !== 'undefined') {
  React.setGlobal({
    userInfo: getInitUserInfo(undefined)
  });
  ReactDOM.render(<App />, document.querySelector('main'));
}
else {
  Global.AppComponent = (props: any) => {
  React.setGlobal({
    userInfo: getInitUserInfo(props ? props.userInfo : undefined)
  });
  return <App {...props} />;
  }
}