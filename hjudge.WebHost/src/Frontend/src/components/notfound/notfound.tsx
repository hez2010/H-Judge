import * as React from 'react';
import { Header } from 'semantic-ui-react';
import { setTitle } from '../../utils/titleHelper';

const NotFound = () => {
  setTitle('错误');
  return (
    <>
      <Header as='h1'>出现错误</Header>
      <Header as='h4' color='red'>你想要找的页面不见了</Header>
    </>
  );
}
export default NotFound;