import * as React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import { UserInfo } from './interfaces/userInfo';

PromisePolyfill.polyfill();

let Global = global as any;

Global.React = React;
Global.ReactDOM = ReactDOM;
Global.ReactDOMServer = ReactDOMServer;
Global.AppComponent = (userInfo: UserInfo, location: string) => <App userInfo={userInfo} location={location} />;

if (typeof window !== 'undefined') ReactDOM.render(<App />, document.querySelector('main'));