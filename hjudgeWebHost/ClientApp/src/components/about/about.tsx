import * as React from "react";
import { Header, Container, Icon } from "semantic-ui-react";
import { setTitle } from "../../utils/titleHelper";

export default class About extends React.Component {
  componentDidMount() {
    setTitle('关于');
  }
  render() {
    return (
      <>
        <Header as='h1'>关于 H::Judge</Header>
      </>
    );
  }
}