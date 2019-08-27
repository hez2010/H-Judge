import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Header, Divider, Grid, Feed, Icon } from 'semantic-ui-react';

const Home = () => {
  setTitle('主页');
  return (
    <>
      <Header as='h1'>欢迎来到 H::Judge</Header>
      <Divider />
      <p>主页当前正在设计中，如果您有任何的建议或设计，欢迎提出或者投稿，联系方式请见页面底部</p>
    </>
  );
}

export default Home;