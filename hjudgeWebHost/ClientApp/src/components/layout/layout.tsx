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
  Segment
} from 'semantic-ui-react';

export default class Layout extends Component {
  render () {
    return (
      <div>
        <Menu fixed='top' inverted>
          <Container>
            <NavLink exact to='/' className='header item' isActive={() => false}>
              <Image size='tiny' src='/assets/hjudge.png' style={{ marhinRight: '1.5em' }}></Image>
            </NavLink>
            <NavLink exact to='/' className='item'>主页</NavLink>
            <NavLink to='/problem' className='item'>题库</NavLink>
            <NavLink to='/contest' className='item'>比赛</NavLink>
            <NavLink to='/group' className='item'>小组</NavLink>
            <NavLink to='/message' className='item'>消息</NavLink>
            <NavLink to='/about' className='item'>关于</NavLink>
            <NavLink to='/login' className='right item'>登录</NavLink>
          </Container>
        </Menu>
        <Container style={{ marginTop: '7em' }}>
            {this.props.children}
        </Container>
      </div>
    );
  }
}