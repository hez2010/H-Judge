import * as React from "react";
import { NavLink } from 'react-router-dom';
import { Container, Menu, Icon, Responsive, Header, Button, Visibility, Segment, Sidebar, Grid, List } from 'semantic-ui-react';
import Login from "../account/login";
import Register from "../account/register";
import { Post } from "../../utils/requestHelper";
import { ResultModel } from "../../interfaces/resultModel";
import { CommonProps } from '../../interfaces/commonProps';

interface LayoutState {
  loginModalOpen: boolean,
  registerModalOpen: boolean
}

interface LayoutProps extends CommonProps { }

const getWidth = () => {
  const isSSR = typeof window === 'undefined';
  return isSSR ? Responsive.onlyTablet.minWidth as number : window.innerWidth;
}

interface DesktopContainerState {
  fixed: boolean
}

class DesktopContainer extends React.Component<ContainerProps, DesktopContainerState> {
  constructor(props: any) {
    super(props);

    this.state = {
      fixed: false
    };
  }

  hideFixedMenu = () => this.setState({ fixed: false })
  showFixedMenu = () => this.setState({ fixed: true })

  render() {
    const { children } = this.props
    const { fixed } = this.state

    return (
      <Responsive getWidth={getWidth} minWidth={Responsive.onlyTablet.minWidth}>
        <Visibility
          once={false}
          onBottomPassed={this.showFixedMenu}
          onBottomPassedReverse={this.hideFixedMenu}
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
                <NavLink to='/status' className='item'>状态</NavLink>
                <NavLink to='/rank' className='item'>排名</NavLink>
                <NavLink to='/message' className='item'>消息</NavLink>
                <NavLink to='/discussion' className='item'>讨论</NavLink>
                <NavLink to='/article' className='item'>文章</NavLink>
                <Menu.Item position='right'>
                  {
                    this.props.signedIn ? <>
                      <NavLink to='/user' className={fixed ? 'ui button' : 'ui inverted button'}>门户</NavLink>
                      <Button as='a' onClick={this.props.logout} style={{ marginLeft: '0.5em' }} inverted={!fixed}>退出</Button>
                    </> :
                      <>
                        <Button as='a' onClick={this.props.login} inverted={!fixed}>登录</Button>
                        <Button as='a' onClick={this.props.register} style={{ marginLeft: '0.5em' }} inverted={!fixed}>注册</Button>
                      </>
                  }
                </Menu.Item>
              </Container>
            </Menu>
          </Segment>
        </Visibility>
        {children}

      </Responsive>
    )
  }
}

interface MobileContainerState {
  sidebarOpened: boolean
}

interface ContainerProps {
  login: () => void,
  register: () => void,
  logout: () => void,
  signedIn: boolean
}

class MobileContainer extends React.Component<ContainerProps, MobileContainerState> {
  constructor(props: any) {
    super(props);

    this.state = {
      sidebarOpened: false
    };
  }

  handleSidebarHide = () => this.setState({ sidebarOpened: false })

  handleToggle = () => this.setState({ sidebarOpened: true })

  render() {
    const { children } = this.props
    const { sidebarOpened } = this.state

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
          onHide={this.handleSidebarHide}
          vertical
          visible={sidebarOpened}
        >
          <NavLink exact to='/' className='item'>主页</NavLink>
          <NavLink to='/problem' className='item'>题库</NavLink>
          <NavLink to='/contest' className='item'>比赛</NavLink>
          <NavLink to='/group' className='item'>小组</NavLink>
          <NavLink to='/status' className='item'>状态</NavLink>
          <NavLink to='/rank' className='item'>排名</NavLink>
          <NavLink to='/message' className='item'>消息</NavLink>
          <NavLink to='/discussion' className='item'>讨论</NavLink>
          <NavLink to='/article' className='item'>文章</NavLink>
          {
            this.props.signedIn ? <>
              <NavLink to='/user' className='item'>门户</NavLink>
              <Menu.Item as='a' onClick={this.props.logout}>退出</Menu.Item>
            </> :
              <>
                <Menu.Item as='a' onClick={this.props.login}>登录</Menu.Item>
                <Menu.Item as='a' onClick={this.props.register}>注册</Menu.Item>
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
                <Menu.Item onClick={this.handleToggle}>
                  <Icon name='sidebar' />
                </Menu.Item>
                <Menu.Item position='right'>
                  {
                    this.props.signedIn ? <>
                      <NavLink to='/user' className='ui inverted button'>门户</NavLink>
                      <Button as='a' onClick={this.props.logout} style={{ marginLeft: '0.5em' }} inverted>退出</Button>
                    </> :
                      <>
                        <Button as='a' onClick={this.props.login} inverted>登录</Button>
                        <Button as='a' onClick={this.props.register} style={{ marginLeft: '0.5em' }} inverted>注册</Button>
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
}

interface ResponsiveContainerProps {
  children: React.ReactNode,
  account: ContainerProps
}

const ResponsiveContainer = ({ children, account }: ResponsiveContainerProps) => (
  <>
    <DesktopContainer {...account}>
      {children}
    </DesktopContainer>
    <MobileContainer {...account}>
      {children}
    </MobileContainer>
  </>
)

export default class Layout extends React.Component<LayoutProps, LayoutState> {
  constructor(props: any) {
    super(props);
    this.state = {
      loginModalOpen: false,
      registerModalOpen: false
    }
    this.login = this.login.bind(this);
    this.logout = this.logout.bind(this);
    this.register = this.register.bind(this);
    this.closeLoginModal = this.closeLoginModal.bind(this);
    this.closeRegisterModal = this.closeRegisterModal.bind(this);
  }

  login() {
    this.setState({
      loginModalOpen: true
    } as LayoutState);
  }

  register() {
    this.setState({
      registerModalOpen: true
    } as LayoutState);
  }

  logout() {
    Post('/Account/Logout').then(res => res.json()).then(data => {
      let result = data as ResultModel;
      if (result.succeeded) {
        this.props.openPortal('提示', '退出成功', 'green');
        this.props.refreshUserInfo();
      }
      else {
        this.props.openPortal('错误', `退出失败\n${result.errorMessage} (${result.errorCode})`, 'red');
      }
    })
      .catch(err => {
        this.props.openPortal('错误', '退出失败', 'red');
        console.log(err);
      })
  }

  closeLoginModal() {
    this.setState({
      loginModalOpen: false
    } as LayoutState);
  }

  closeRegisterModal() {
    this.setState({
      registerModalOpen: false
    } as LayoutState);
  }

  render() {
    let content = <>
      <Container style={{ paddingTop: '3em', paddingBottom: '3em' }}>
        {this.props.children}
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
      signedIn: this.props.userInfo.signedIn,
      login: this.login,
      logout: this.logout,
      register: this.register
    } as ContainerProps;

    return (
      <>
        <ResponsiveContainer children={content} account={account} />
        <Login {...this.props} modalOpen={this.state.loginModalOpen} closeModal={this.closeLoginModal} />
        <Register {...this.props} modalOpen={this.state.registerModalOpen} closeModal={this.closeRegisterModal} />
      </>
    );
  }
}