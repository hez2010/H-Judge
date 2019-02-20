import React, { Component } from 'react';
import { Route, Switch } from 'react-router-dom';
import Layout from './components/layout/layout'

export default class App extends Component {
  render() {
    return (
      <div>
        <Layout>
          <Switch>
            <Route exact path='/' render={() => { return <p>hhhh</p>; }}></Route>
          </Switch>
        </Layout>
      </div>
    );
  }
}
