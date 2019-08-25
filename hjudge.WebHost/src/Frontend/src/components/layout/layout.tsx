import * as React from 'react';
import { NavLink } from 'react-router-dom';
import { Container, Menu, Icon, Responsive, Header, Button, Visibility, Segment, Sidebar, Grid, List } from 'semantic-ui-react';
import Login from '../account/login';
import Register from '../account/register';
import { Post } from '../../utils/requestHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { useGlobal } from 'reactn';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { UserInfo } from '../../interfaces/userInfo';
import { getTargetState } from '../../utils/reactnHelper';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { tryJson } from '../../utils/responseHelper';
import { getWidth } from '../../utils/windowHelper';

const DesktopContainer = (props: ContainerProps & React.Props<never>) => {
  const [fixed, setFixed] = React.useState<boolean>(false);

  const hideFixedMenu = () => setFixed(false);
  const showFixedMenu = () => setFixed(true);
  const { children } = props;

  return (
    <Responsive getWidth={getWidth} minWidth={Responsive.onlyTablet.minWidth}>
      <Visibility
        once={false}
        onBottomPassed={showFixedMenu}
        onBottomPassedReverse={hideFixedMenu}
      >
        <Segment
          inverted
          textAlign='center'
          style={{ padding: '1em 0em' }}
          vertical
        >
          <Menu
            fixed={fixed ? 'top' : undefined}
            inverted={!fixed}
            pointing={!fixed}
            secondary={!fixed}
            size='large'
          >
            <Container>
              <NavLink exact to='/' className='item'>主页</NavLink>
              <NavLink to='/problem' className='item'>题库</NavLink>
              <NavLink to='/contest' className='item'>比赛</NavLink>
              <NavLink to='/group' className='item'>小组</NavLink>
              <NavLink to='/statistics' className='item'>状态</NavLink>
              <NavLink to='/message' className='item'>消息</NavLink>
              <NavLink to='/discussion' className='item'>讨论</NavLink>
              <NavLink to='/article' className='item'>文章</NavLink>
              <Menu.Item position='right'>
                {
                  props.signedIn ? <>
                    <NavLink to='/user' className={fixed ? 'ui button' : 'ui inverted button'}>门户</NavLink>
                    <Button as='a' onClick={props.logout} style={{ marginLeft: '0.5em' }} inverted={!fixed}>退出</Button>
                  </> :
                    <>
                      <Button as='a' onClick={props.login} inverted={!fixed}>登录</Button>
                      <Button as='a' onClick={props.register} style={{ marginLeft: '0.5em' }} inverted={!fixed}>注册</Button>
                    </>
                }
              </Menu.Item>
            </Container>
          </Menu>
        </Segment>
      </Visibility>
      {children}
    </Responsive>
  );
}

interface ContainerProps {
  login: () => void,
  register: () => void,
  logout: () => void,
  signedIn: boolean
}

const MobileContainer = (props: ContainerProps & React.Props<never>) => {
  const [sidebarOpened, setSidebarOpened] = React.useState<boolean>(false);

  const handleSidebarHide = () => setSidebarOpened(false);

  const handleToggle = () => setSidebarOpened(true);
  const { children } = props

  return (
    <Responsive
      as={Sidebar.Pushable}
      getWidth={getWidth}
      maxWidth={Responsive.onlyMobile.maxWidth}
    >
      <Sidebar
        as={Menu}
        animation='push'
        inverted
        onHide={handleSidebarHide}
        vertical
        visible={sidebarOpened}
      >
        <NavLink exact to='/' className='item'>主页</NavLink>
        <NavLink to='/problem' className='item'>题库</NavLink>
        <NavLink to='/contest' className='item'>比赛</NavLink>
        <NavLink to='/group' className='item'>小组</NavLink>
        <NavLink to='/statistics' className='item'>状态</NavLink>
        <NavLink to='/message' className='item'>消息</NavLink>
        <NavLink to='/discussion' className='item'>讨论</NavLink>
        <NavLink to='/article' className='item'>文章</NavLink>
        {
          props.signedIn ? <>
            <NavLink to='/user' className='item'>门户</NavLink>
            <Menu.Item as='a' onClick={props.logout}>退出</Menu.Item>
          </> :
            <>
              <Menu.Item as='a' onClick={props.login}>登录</Menu.Item>
              <Menu.Item as='a' onClick={props.register}>注册</Menu.Item>
            </>
        }
      </Sidebar>

      <Sidebar.Pusher dimmed={sidebarOpened}>
        <Segment
          inverted
          textAlign='center'
          style={{ padding: '1em 0em' }}
          vertical
        >
          <Container>
            <Menu inverted pointing secondary size='large'>
              <Menu.Item onClick={handleToggle}>
                <Icon name='sidebar' />
              </Menu.Item>
              <Menu.Item position='right'>
                {
                  props.signedIn ? <>
                    <NavLink to='/user' className='ui inverted button'>门户</NavLink>
                    <Button as='a' onClick={props.logout} style={{ marginLeft: '0.5em' }} inverted>退出</Button>
                  </> :
                    <>
                      <Button as='a' onClick={props.login} inverted>登录</Button>
                      <Button as='a' onClick={props.register} style={{ marginLeft: '0.5em' }} inverted>注册</Button>
                    </>
                }
              </Menu.Item>
            </Menu>
          </Container>
        </Segment>
        {children}
      </Sidebar.Pusher>
    </Responsive>
  )
}
interface ResponsiveContainerProps {
  account: ContainerProps
}

const ResponsiveContainer = (props: ResponsiveContainerProps & React.Props<never>) => <>
  <DesktopContainer {...props.account}>
    {props.children}
  </DesktopContainer>
  <MobileContainer {...props.account}>
    {props.children}
  </MobileContainer>
</>;

const Layout = (props: CommonProps & React.Props<never>) => {
  const [loginModalOpen, setLoginModalOpen] = React.useState(false);
  const [registerModalOpen, setRegisterModalOpen] = React.useState(false);
  const [userInfo] = getTargetState<UserInfo>(useGlobal<GlobalState>('userInfo'));
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  const login = () => setLoginModalOpen(true);

  const register = () => setRegisterModalOpen(true);

  const logout = () => {
    Post('/user/logout').then(tryJson).then(data => {
      let error = data as ErrorModel;
      if (error.errorCode) {
        commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        return;
      }
      commonFuncs.openPortal('提示', '退出成功', 'green');
      commonFuncs.refreshUserInfo();
    })
      .catch(err => {
        commonFuncs.openPortal('错误', '退出失败', 'red');
        console.log(err);
      })
  }

  const closeLoginModal = () => setLoginModalOpen(false);

  const closeRegisterModal = () => setRegisterModalOpen(false);

  const content = <>
    <Container style={{ paddingTop: '3em', paddingBottom: '3em' }}>
      {props.children}
    </Container>
    <Segment inverted vertical style={{ padding: '5em 0em' }}>
      <Container>
        <Grid divided inverted stackable>
          <Grid.Row>
            <Grid.Column width={4}>
              <Header inverted as='h4' content='关于 H::Judge' />
              <List link inverted>
                <NavLink to='/about' className='item'>更新日志</NavLink>
                <List.Item as='a'>开发者：hez2010</List.Item>
              </List>
            </Grid.Column>
            <Grid.Column width={4}>
              <Header inverted as='h4' content='联系方式' />
              <List link inverted>
                <List.Item as='a' href='mailto:hez2010@126.com'>Email</List.Item>
                <List.Item as='a' href='https://github.com/hez2010/H-Judge'>GitHub</List.Item>
              </List>
            </Grid.Column>
            <Grid.Column width={6}>
              <Header as='h4' inverted>您的专属评测系统</Header>
              <p>H::Judge &copy; {new Date(Date.now()).getFullYear()}. hez2010 版权所有</p>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </Segment>
  </>;

  let account = {
    signedIn: userInfo.signedIn,
    login: login,
    logout: logout,
    register: register
  } as ContainerProps;

  return (
    <>
      <ResponsiveContainer children={content} account={account} />
      <Login modalOpen={loginModalOpen} closeModal={closeLoginModal} {...props} />
      <Register modalOpen={registerModalOpen} closeModal={closeRegisterModal} {...props} />
    </>
  );
};

export default Layout;