import * as React from "react";
import { UserInfo } from "../../interfaces/userInfo";
import { setTitle } from "../../utils/titleHelper";
import { Header } from "semantic-ui-react";

export default class Home extends React.Component<PropsInterface> {
  componentDidMount() {
    setTitle('主页');
  }

  render() {
    return (
      <>
        <Header as='h1'>欢迎来到 H::Judge</Header>
      </>
    );
  }
}

export interface PropsInterface {
  userInfo: UserInfo
}
