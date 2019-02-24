import * as React from "react";
import { NavLink } from 'react-router-dom';
import { Container, Menu, Icon, TransitionablePortal, Segment, Header, SemanticCOLORS, Dropdown } from 'semantic-ui-react';
import { UserInfo } from "../../interfaces/userInfo";

interface PortalState {
  open: boolean,
  header: string,
  message: string,
  color: SemanticCOLORS
}

interface LayoutState {
  portal: PortalState

}

interface LayoutProps {
  userInfo: UserInfo
}

export default class Layout extends React.Component<LayoutProps, LayoutState> {
  constructor(props: any) {
    super(props);
    this.state = {
      portal: {
        open: false,
        header: '',
        message: '',
        color: 'black'
      }
    }
  }
  openPortal(header: string, message: string, color: string) {
    if (this.state.portal.open) {
      this.setState({
        portal: {
          open: false
        }
      } as LayoutState);
    }
    process.nextTick(() => {
      this.setState({
        portal: {
          open: true,
          header: header,
          message: message,
          color: color
        }
      } as LayoutState);
    })
  }

  render() {
    let accountOptions = this.props.userInfo.signedIn ? <Dropdown text='账户' floating>
      <Dropdown.Menu>
        <Dropdown.Item icon='log out' text='门户' />
        <Dropdown.Item icon='log out' text='退出' />
      </Dropdown.Menu>
    </Dropdown> : <Dropdown text='账户' floating>
        <Dropdown.Menu>
          <Dropdown.Item icon='log out' text='登录' />
          <Dropdown.Item icon='log out' text='注册' />
        </Dropdown.Menu>
      </Dropdown>;

    return (
      <div>
        <Menu fixed='top' size='small' borderless inverted color='blue' compact icon='labeled'>
          <Container>
            <NavLink exact to='/' className='item'><Icon name='h'></Icon>主页</NavLink>
            <NavLink to='/problem' className='item'><Icon name='code'></Icon>题库</NavLink>
            <NavLink to='/contest' className='item'><Icon name='pencil'></Icon>比赛</NavLink>
            <NavLink to='/group' className='item'><Icon name='users'></Icon>小组</NavLink>
            <NavLink to='/status' className='item'><Icon name='list'></Icon>状态</NavLink>
            <NavLink to='/rank' className='item'><Icon name='list ol'></Icon>排名</NavLink>
            <NavLink to='/message' className='item'><Icon name='paper plane'></Icon>消息</NavLink>
            <NavLink to='/discussion' className='item'><Icon name='discussions'></Icon>讨论</NavLink>
            <NavLink to='/article' className='item'><Icon name='book'></Icon>文章</NavLink>
            <NavLink to='/about' className='item'><Icon name='info circle'></Icon>关于</NavLink>
            <Menu.Item position='right'>
              <Icon name='user circle'></Icon>
              {accountOptions}
            </Menu.Item>
          </Container>
        </Menu>
        <Container style={{ marginTop: '7em' }}>
          {this.props.children}
        </Container>
        <Menu fixed='bottom' inverted color='blue' borderless>
          <Container>
            <Menu.Item>
              H::Judge &copy; {new Date().getFullYear()}
            </Menu.Item>
            <Menu.Item position='right'>
              hez2010 All Rights Reserved.
            </Menu.Item>
          </Container>
        </Menu>
        <TransitionablePortal open={this.state.portal.open} onClose={() => this.setState({ portal: { open: false } } as LayoutState)} transition={{ animation: 'drop', duration: 500 }}>
          <Segment style={{ bottom: '5em', position: 'fixed', right: '2em' }} color={this.state.portal.color} inverted>
            <Header>{this.state.portal.header}</Header>
            <p style={{ wordBreak: 'break-all', wordWrap: 'break-word', 'overflow': 'hidden', width: '20em' }}>{this.state.portal.message}</p>
          </Segment>
        </TransitionablePortal>
      </div>
    );
  }
}