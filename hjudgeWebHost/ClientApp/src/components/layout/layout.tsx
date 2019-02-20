import React, { Component } from 'react';
export default class Layout extends Component {
  render () {
    return (
      <div>
        <h1>hello</h1>
        <div>
            {this.props.children}
        </div>
      </div>
    );
  }
}