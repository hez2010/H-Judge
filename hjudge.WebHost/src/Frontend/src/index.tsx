import * as React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import App from './App';
import * as PromisePolyfill from 'es6-promise';
import "regenerator-runtime/runtime";

// polyfill promise and fetch for SSR
PromisePolyfill.polyfill();

let Global = global as any;
Global.ReactDOM = ReactDOM;
Global.React = React;
Global.ReactDOMServer = ReactDOMServer;

// exposed for SSR interop
Global.AppComponent = (props: any) =>  <App {...props} />;

if (typeof window !== 'undefined') {
  ReactDOM.render(<App />, document.querySelector('main'));
}
