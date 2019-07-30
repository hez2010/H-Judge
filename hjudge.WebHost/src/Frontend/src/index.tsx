import * as React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import { UserInfo } from './interfaces/userInfo';

PromisePolyfill.polyfill();

(global as any).AppComponent = (userInfo: UserInfo) => <App userInfo={userInfo} />;
(global as any).React = React;
(global as any).ReactDOM = ReactDOM;
(global as any).ReactDOMServer = ReactDOMServer;