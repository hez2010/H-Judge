import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Header, Divider } from 'semantic-ui-react';
import { CommonProps } from '../../interfaces/commonProps';

interface HomeProps extends CommonProps { }

export default class Home extends React.Component<HomeProps> {
  componentDidMount() {
    setTitle('主页');
  }

  render() {
    return (
      <>
        <Header as='h1'>欢迎来到 H::Judge</Header>
        <p>这是即将发布的 H::Judge 全新版本，会跟随开发进度不断更新。</p>
        <p>但是由于仍处于开发中，会出现没有完成的模块以及一些 bug。</p>
        <Divider />
        <p>主页放些什么东西好呢。。。。。</p>
      </>
    );
  }
}
