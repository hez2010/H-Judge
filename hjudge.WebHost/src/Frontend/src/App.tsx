﻿import * as React from 'react';
import { Route, Switch, BrowserRouter, StaticRouter } from 'react-router-dom';
import Layout from './components/layout/layout';
import NotFound from './components/notfound/notfound';
import About from './components/about/about';
import Home from './components/home/home';
import { UserInfo } from './interfaces/userInfo'
import { Get } from './utils/requestHelper';
import User from './components/account/user';
import { TransitionablePortal, Header, Segment, Divider, SemanticCOLORS, Icon } from 'semantic-ui-react';
import Problem from './components/problem/problem';
import Contest from './components/contest/contest';
import ProblemDetails from './components/problem/details';
import Group from './components/group/group';
import ContestDetails from './components/contest/details';
import Statistics from './components/statistics/statistics';
import ProblemEdit from './components/problem/edit';
import ContestEdit from './components/contest/edit';
import AppContext from './AppContext';
import { ErrorModel } from './interfaces/errorModel';
import { tryJson } from './utils/responseHelper';
import Result from './components/result/result';
import { CommonFuncs } from './interfaces/commonFuncs';

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


interface PortalState {
  open: boolean,
  header: string,
  message: string,
  color: SemanticCOLORS
}

const App = (props: any) => {
  const [userInfo, setUserInfo] = React.useState<UserInfo>(typeof window === 'undefined' ? getInitUserInfo(undefined) : getInitUserInfo(props ? props.userInfo : undefined));

  const openPortal = (header: string, message: string, color: SemanticCOLORS) => {
    if (portal.open) {
      portal.open = false;
      setPortal({ ...portal });
    }
    process.nextTick(() => {
      portal.open = true;
      portal.header = header;
      portal.message = message;
      portal.color = color;

      setPortal({ ...portal });
    })
  }

  const closePortal = () => {
    portal.open = false;
    setPortal({ ...portal });
  }

  const refreshUserInfo = () => {
    Get('/user/profiles')
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as UserInfo;
        setUserInfo(result);
      })
      .catch(err => {
        openPortal('错误', '用户信息加载失败', 'red');
        console.error(err);
      });
  }

  const [commonFuncs, setCommonFuncs] = React.useState<CommonFuncs>({ openPortal, refreshUserInfo });
  const [portal, setPortal] = React.useState<PortalState>({ open: false, header: '', message: '', color: 'green' });

  React.useEffect(() => {
    setCommonFuncs({ openPortal: openPortal, refreshUserInfo: refreshUserInfo });
    if (!userInfo.userId) refreshUserInfo();
  }, []);

  const renderContent = <AppContext.Provider value={{ userInfo, setUserInfo, commonFuncs }}>
    <Layout>
      <Switch>
        <Route
          exact
          path='/'
          component={Home}>
        </Route>
        <Route
          path='/problem/:page?'
          component={Problem}>
        </Route>
        <Route
          path='/details/problem/:problemId/:contestId?/:groupId?'
          component={ProblemDetails}>
        </Route>
        <Route
          path='/edit/problem/:problemId'
          component={ProblemEdit}>
        </Route>
        <Route
          path='/contest/:page?'
          component={Contest}>
        </Route>
        <Route
          path='/details/contest/:contestId/:groupId?'
          component={ContestDetails}>
        </Route>
        <Route
          path='/edit/contest/:contestId'
          component={ContestEdit}>
        </Route>
        <Route
          path='/group/:page?'
          component={Group}>
        </Route>
        <Route
          path='/message'
          component={() => <p>此功能正在开发中，敬请期待</p>}>
        </Route>
        <Route
          path='/statistics/:userName/:groupId/:contestId/:problemId/:result/:page?'
          component={Statistics}>
        </Route>
        <Route
          path='/statistics/:page?'
          component={Statistics}>
        </Route>
        <Route
          path='/result/:resultId'
          component={Result}>
        </Route>
        <Route
          path='/discussion'
          component={() => <p>此功能正在开发中，敬请期待</p>}>
        </Route>
        <Route
          path='/article'
          component={() => <p>此功能正在开发中，敬请期待</p>}>
        </Route>
        <Route
          exact
          path='/about'
          component={About}>
        </Route>
        <Route
          path='/user/:userId?'
          component={User}>
        </Route>
        <Route
          component={NotFound}>
        </Route>
      </Switch>
    </Layout>
    <TransitionablePortal open={portal.open} onClose={closePortal} transition={{ animation: 'drop', duration: 500 }}>
      <Segment style={{ bottom: '5em', position: 'fixed', right: '2em', zIndex: 1048576, maxHeight: '10em', width: '20em' }} color={portal.color} inverted>
        <Header>
          {portal.header}
          <div style={{ display: 'inline', cursor: 'pointer', float: 'right' }} onClick={closePortal}>
            <Icon name='close' size='small'></Icon>
          </div>
        </Header>
        <Divider />
        <pre style={{ wordBreak: 'break-all', wordWrap: 'break-word', overflow: 'auto', scrollBehavior: 'auto', width: '18em', maxHeight: '5em' }}>{portal.message}</pre>
      </Segment>
    </TransitionablePortal>
  </AppContext.Provider>;

  if (typeof window === 'undefined') return <StaticRouter context={props.context} location={props.location}>{renderContent}</StaticRouter>;
  return <BrowserRouter>{renderContent}</BrowserRouter>;
};

export default App;
