import * as React from "react";
import { Route, Switch, BrowserRouter, StaticRouter } from 'react-router-dom';
import Layout from './components/layout/layout';
import NotFound from './components/notfound/notfound';
import About from './components/about/about';
import Home from './components/home/home';
import { UserInfo } from './interfaces/userInfo'
import { Get } from "./utils/requestHelper";
import User from "./components/account/user";
import { TransitionablePortal, Header, Segment, Divider, SemanticCOLORS, Icon } from "semantic-ui-react";
import Problem from "./components/problem/problem";
import Contest from "./components/contest/contest";
import ProblemDetails from "./components/problem/details";
import Group from "./components/group/group";
import { CommonProps } from "./interfaces/commonProps";
import ContestDetails from "./components/contest/details";

interface PortalState {
  open: boolean,
  header: string,
  message: string,
  color: SemanticCOLORS
}

interface AppState {
  userInfo: UserInfo,
  portal: PortalState
}

interface AppProps {
  userInfo?: UserInfo,
  location?: string
}

export default class App extends React.Component<AppProps, AppState> {
  constructor(props: any) {
    super(props);

    this.state = {
      portal: {
        open: false,
        header: '',
        message: '',
        color: 'black'
      },
      userInfo:
        this.props.userInfo ?
          this.props.userInfo :
          {
            userId: "",
            userName: "",
            privilege: 4,
            name: "",
            email: "",
            signedIn: false,
            emailConfirmed: false,
            coins: 0,
            experience: 0,
            otherInfo: [],
            phoneNumber: "",
            phoneNumberConfirmed: false
          }
    };

    this.refreshUserInfo = this.refreshUserInfo.bind(this);
    this.openPortal = this.openPortal.bind(this);
    this.closePortal = this.closePortal.bind(this);

    if (!this.props.userInfo) this.refreshUserInfo();
  }

  openPortal(header: string, message: string, color: SemanticCOLORS) {
    if (this.state.portal.open) {
      this.setState({
        portal: {
          open: false
        }
      } as AppState);
    }
    process.nextTick(() => {
      this.setState({
        portal: {
          open: true,
          header: header,
          message: message,
          color: color
        }
      } as AppState);
    })
  }

  closePortal() {
    this.setState({
      portal: {
        open: false
      }
    } as AppState);
  }

  refreshUserInfo() {
    Get('/Account/UserInfo')
      .then(response => response.json())
      .then(data => {
        this.setState({
          userInfo: data
        } as AppState);
      })
      .catch(err => {
        this.openPortal('错误', '用户信息加载失败', 'red');
        console.error(err);
      });
  }

  renderContent() {
    let commonProps = {
      openPortal: this.openPortal,
      refreshUserInfo: this.refreshUserInfo,
      userInfo: this.state.userInfo
    } as CommonProps;

    return <>
      <Layout {...commonProps}>
        <Switch>
          <Route
            exact
            path='/'
            render={props => <Home {...Object.assign(commonProps, props)} />}>
          </Route>
          <Route
            path='/problem/:page?'
            render={props => <Problem {...Object.assign(commonProps, props)}></Problem>}>
          </Route>
          <Route
            path='/details/problem/:problemId/:contestId?/:groupId?'
            render={props => <ProblemDetails {...Object.assign(commonProps, props)}></ProblemDetails>}>
          </Route>
          <Route
            path='/details/contest/:contestId?/:groupId?'
            render={props => <ContestDetails {...Object.assign(commonProps, props)}></ContestDetails>}>
          </Route>
          <Route
            path='/contest/:page?'
            render={props => <Contest {...Object.assign(commonProps, props)}></Contest>}>
          </Route>
          <Route
            path='/group/:page?'
            render={props => <Group {...Object.assign(commonProps, props)}></Group>}>
          </Route>
          <Route
            path='/message'
            render={props => <p {...Object.assign(commonProps, props)}>message</p>}>
          </Route>
          <Route
            path='/status'
            render={props => <p {...Object.assign(commonProps, props)}>status</p>}>
          </Route>
          <Route
            path='/rank'
            render={props => <p {...Object.assign(commonProps, props)}>rank</p>}>
          </Route>
          <Route
            path='/discussion'
            render={props => <p {...Object.assign(commonProps, props)}>discussion</p>}>
          </Route>
          <Route
            path='/article'
            render={props => <p {...Object.assign(commonProps, props)}>article</p>}>
          </Route>
          <Route
            exact
            path='/about'
            component={About}>
          </Route>
          <Route
            path='/user'
            render={props => <User {...Object.assign(commonProps, props)} />}>
          </Route>
          <Route
            component={NotFound}>
          </Route>
        </Switch>
      </Layout>
      <TransitionablePortal open={this.state.portal.open} onClose={this.closePortal} transition={{ animation: 'drop', duration: 500 }}>
        <Segment style={{ bottom: '5em', position: 'fixed', right: '2em', zIndex: 1048576 }} color={this.state.portal.color} inverted>
          <Header>
            {this.state.portal.header}
            <div style={{ display: 'inline', cursor: 'pointer', float: 'right' }} onClick={this.closePortal}>
              <Icon name='close' size='small'></Icon>
            </div>
          </Header>
          <Divider />
          <p style={{ wordBreak: 'break-all', wordWrap: 'break-word', 'overflow': 'hidden', width: '20em' }}>{this.state.portal.message}</p>
        </Segment>
      </TransitionablePortal>
    </>;
  }

  render() {
    if (typeof window === 'undefined') return <StaticRouter location={this.props.location}>{this.renderContent()}</StaticRouter>;
    return <BrowserRouter>{this.renderContent()}</BrowserRouter>;
  }
}
