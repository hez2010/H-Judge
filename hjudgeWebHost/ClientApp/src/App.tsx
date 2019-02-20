import React, { Component } from 'react';
import { Route, Switch } from 'react-router-dom';
import Layout from './components/layout/layout';
import 'semantic-ui-css/semantic.min.css';

export default class App extends Component {
  render() {
    return (
      <div>
        <Layout>
          <Switch>
            <Route exact path='/' render={() => { return <p>home</p>; }}></Route>
            <Route path='/problem' render={() => { return <p>problem</p>; }}></Route>
            <Route path='/contest' render={() => { return <p>contest</p>; }}></Route>
            <Route path='/group' render={() => { return <p>group</p>; }}></Route>
            <Route path='/message' render={() => { return <p>message</p>; }}></Route>
            <Route exact path='/about' render={() => { return <p>about</p>; }}></Route>
            <Route render={() => {  return <p>404</p>; }}></Route>
          </Switch>
        </Layout>
      </div>
    );
  }
}
