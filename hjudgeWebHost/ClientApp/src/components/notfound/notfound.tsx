import * as React from "react";
import { Header } from 'semantic-ui-react';
import { setTitle } from '../../utils/titleHelper';

export default class NotFound extends React.Component {
  componentDidMount() {
    setTitle('错误');
  }

  render() {
    return (
      <div>
        <Header as='h1'>出现错误</Header>
        <Header as='h4' color='red'>你想要找的页面不见了</Header>
      </div>
    );
  }
}