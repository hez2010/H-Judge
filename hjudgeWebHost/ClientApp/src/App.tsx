import * as React from "react";
import { Route, Switch } from 'react-router-dom';
import Layout from './components/layout/layout';
import 'semantic-ui-css/semantic.min.css';
import NotFound from './components/notfound/notfound';
import About from './components/about/about';
import Home, { PropsInterface } from './components/home/home';
import { UserInfo } from './interfaces/userInfo'
import { Post } from "./utils/requestHelper";

interface AppState {
  userInfo: UserInfo
}

export default class App extends React.Component<{}, AppState> {
  layoutRef: React.RefObject<Layout>;

  constructor(props: {}) {
    super(props);
    this.layoutRef = React.createRef<Layout>();
    this.state = {
      userInfo: {
        email: '',
        isEmailConfirmed: false,
        isPhoneNumberConfirmed: false,
        name: '',
        otherInfo: [],
        phoneNumber: '',
        privilege: 0,
        userId: '',
        userName: ''
      }
    }

    Post('/Account/GetUserInfo')
      .then(response => response.json())
      .then(data => this.setState({
        userInfo: data
      } as AppState))
      .catch(err => {
        if (this.layoutRef.current !== null)
          this.layoutRef.current.openPortal('错误', '用户信息加载失败', 'red');
        console.error(err);
      });
  }

  render() {
    return (
      <div>
        <Layout ref={this.layoutRef}>
          <Switch>
            <Route
              exact
              path='/'
              render={(props) => <Home {...props} userInfo={this.state.userInfo} />}>
            </Route>
            <Route
              path='/problem'
              render={() => { return <p>problem</p>; }}>
            </Route>
            <Route
              path='/contest'
              render={() => { return <p>contest</p>; }}>
            </Route>
            <Route
              path='/group'
              render={() => { return <p>group</p>; }}>
            </Route>
            <Route
              path='/message'
              render={() => { return <p>message</p>; }}>
            </Route>
            <Route
              path='/status'
              render={() => { return <p>status</p>; }}>
            </Route>
            <Route
              path='/rank'
              render={() => { return <p>rank</p>; }}>
            </Route>
            <Route
              path='/discussion'
              render={() => { return <p>discussion</p>; }}>
            </Route>
            <Route
              path='/article'
              render={() => { return <p>article</p>; }}>
            </Route>
            <Route
              exact
              path='/about'
              component={About}>
            </Route>
            <Route
              component={NotFound}>
            </Route>
          </Switch>
        </Layout>
      </div>
    );
  }
}
