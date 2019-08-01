import * as React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';

PromisePolyfill.polyfill();

let Global = global as any;

Global.React = React;
Global.ReactDOM = ReactDOM;
Global.ReactDOMServer = ReactDOMServer;
Global.AppComponent = (props: any) => <App {...props} />;

if (typeof window !== 'undefined') ReactDOM.render(<App />, document.querySelector('main'));