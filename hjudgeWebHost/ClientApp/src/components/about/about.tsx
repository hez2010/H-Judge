import React, { Component } from "react";
import { Header, Container } from "semantic-ui-react";
import { setTitle } from "../../utils/titleHelper";

export default class About extends Component {
  componentDidMount() {
    setTitle('关于');
  }
  render() {
    return (
      <div>
        <Header as='h1'>关于 H::Judge</Header>
        <Header as='h3' dividing>更新历程</Header>

      </div>
    );
  }
}