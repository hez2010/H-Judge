import * as React from 'reactn';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import "regenerator-runtime/runtime";
import { UserInfo } from './interfaces/userInfo';

PromisePolyfill.polyfill();

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
  let Global = global as any;

  Global.React = React;
  Global.ReactDOM = ReactDOM;
  Global.ReactDOMServer = ReactDOMServer;
  Global.AppComponent = (props: any) => {
    React.setGlobal({
      userInfo: getInitUserInfo(props ? props.userInfo : undefined)
    });
    return <App {...props} />;
  }
}