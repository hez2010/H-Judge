import React, { Component } from 'react';
import { NavLink } from 'react-router-dom';
import {
  Container,
  Divider,
  Dropdown,
  Grid,
  Header,
  Image,
  List,
  Menu,
  Segment,
  Icon
} from 'semantic-ui-react';

export default class Layout extends Component {
  render() {
    return (
      <div>
        <Menu fixed='top' borderless inverted color='blue' compact icon='labeled'>
          <Container>
            <NavLink exact to='/' className='item'><Icon name='home'></Icon>主页</NavLink>
            <NavLink to='/problem' className='item'><Icon name='code'></Icon>题库</NavLink>
            <NavLink to='/contest' className='item'><Icon name='pencil'></Icon>比赛</NavLink>
            <NavLink to='/group' className='item'><Icon name='users'></Icon>小组</NavLink>
            <NavLink to='/status' className='item'><Icon name='list'></Icon>状态</NavLink>
            <NavLink to='/rank' className='item'><Icon name='list ol'></Icon>排名</NavLink>
            <NavLink to='/message' className='item'><Icon name='paper plane'></Icon>消息</NavLink>
            <NavLink to='/about' className='item'><Icon name='info circle'></Icon>关于</NavLink>
            <NavLink to='/login' className='right item'><Icon name='user circle'></Icon>登录</NavLink>
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
      </div>
    );
  }
}