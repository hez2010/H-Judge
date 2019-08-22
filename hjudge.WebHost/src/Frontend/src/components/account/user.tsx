import { Item, Popup, Input, Divider, Header, Icon, Table, Label, Form, Placeholder, Grid } from 'semantic-ui-react';
import { OtherInfo, UserInfo } from '../../interfaces/userInfo';
import { setTitle } from '../../utils/titleHelper';
import { Post, Get } from '../../utils/requestHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { NavLink } from 'react-router-dom';
import * as React from 'react';
import { useGlobal } from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { getTargetState } from '../../utils/reactnHelper';
import { CommonProps } from '../../interfaces/commonProps';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';

interface ProblemStatisticsModel {
  solvedProblems: number[],
  triedProblems: number[],
  loaded: boolean
}

const User = (props: CommonProps) => {
  const [userInfo] = getTargetState<UserInfo>(useGlobal<GlobalState>('userInfo'));
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));
  const [solvedProblems, setSolvedProblems] = React.useState<number[]>([]);
  const [triedProblems, setTiredProblems] = React.useState<number[]>([]);
  const [loaded, setLoaded] = React.useState<boolean>(false);

  const confirmEmail = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  const confirmPhoneNumber = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  const changeAvatar = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    let element = event.target as HTMLInputElement;
    let info: any = {};
    switch (element.name) {
      case 'name':
      case 'email':
      case 'phoneNumber':
        info[element.name] = element.value;
        break;
      default:
        let otherInfo: OtherInfo[] = [];
        otherInfo.push({ key: element.name, value: element.value, name: '' });
        info.otherInfo = otherInfo;
        break;
    }

    Post('/user/profiles', info)
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        commonFuncs.openPortal('提示', '信息更新成功', 'green');
        commonFuncs.refreshUserInfo();
      })
      .catch(err => {
        commonFuncs.openPortal('错误', '信息更新失败', 'red');
        console.log(err);
      });
  }

  const loadProblems = () => {
    Get('/user/stats')
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemStatisticsModel;
        setSolvedProblems(result.solvedProblems);
        setTiredProblems(result.triedProblems);
        setLoaded(true);
      })
      .catch(err => {
        commonFuncs.openPortal('错误', '做题记录加载失败', 'red');
        console.log(err);
      });
  }

  const renderProblems = () => {
    if (!loaded) {
      return <>
        <Header as='h5'>已通过的题目</Header>
        <Placeholder>
          <Placeholder.Paragraph>
            <Placeholder.Line />
            <Placeholder.Line />
            <Placeholder.Line />
            <Placeholder.Line />
          </Placeholder.Paragraph>
        </Placeholder>
        <Header as='h5'>已尝试的题目</Header>
        <Placeholder>
          <Placeholder.Paragraph>
            <Placeholder.Line />
            <Placeholder.Line />
            <Placeholder.Line />
            <Placeholder.Line />
          </Placeholder.Paragraph>
        </Placeholder></>;
    }
    let solved = solvedProblems.length === 0 ? <p>无</p> :
      <Grid columns={10}>{solvedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;
    let tried = triedProblems.length === 0 ? <p>无</p> :
      <Grid columns={10}>{triedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;

    return <>
      <Header as='h5'>已通过的题目</Header>
      {solved}
      <Header as='h5'>已尝试的题目</Header>
      {tried}
    </>;
  }

  const showUserInfo = () => {
    return <>
      <Item.Group>
        <Item>
          <div>
            <Popup
              position='bottom center'
              trigger={<Item.Image size='small' src={`/user/avatar?userId=${userInfo.userId}`} circular style={{ cursor: 'pointer' }} onClick={changeAvatar} />}
              content='点击更换头像'
            />
          </div>
          <Item.Content>
            <Item.Header>{userInfo.userName}</Item.Header>
            <Item.Meta><Label>{userInfo.privilege === 1 ? '管理员' :
              userInfo.privilege === 2 ? '教师' :
                userInfo.privilege === 3 ? '助教' :
                  userInfo.privilege === 4 ? '学生' :
                    userInfo.privilege === 5 ? '黑名单' : '未知'}</Label> 经验：{userInfo.experience}，金币：{userInfo.coins}</Item.Meta>
            <Item.Description>

              <Grid columns={2} relaxed='very' stackable>
                <Grid.Column>
                  <Form id='infoForm'>
                    <Divider horizontal>
                      <Header as='h4'>
                        <Icon name='tag'></Icon>
                        基本信息
                      </Header>
                    </Divider>
                    <Table definition>
                      <Table.Body>
                        <Table.Row>
                          <Table.Cell textAlign='center' width={4}>姓名</Table.Cell>
                          <Table.Cell><Input name='name' onBlur={handleChange} fluid defaultValue={userInfo.name} /></Table.Cell>
                        </Table.Row>
                        <Table.Row>
                          <Table.Cell textAlign='center' width={4}>邮箱</Table.Cell>
                          <Table.Cell><Input name='email' onBlur={handleChange} fluid defaultValue={userInfo.email} type='email' action={userInfo.emailConfirmed ? null : { primary: true, content: '验证', onClick: confirmEmail }} /></Table.Cell>
                        </Table.Row>
                        <Table.Row>
                          <Table.Cell textAlign='center' width={4}>手机</Table.Cell>
                          <Table.Cell><Input name='phoneNumber' onBlur={handleChange} fluid defaultValue={userInfo.phoneNumber} action={userInfo.phoneNumberConfirmed ? null : { primary: true, content: '验证', onClick: confirmPhoneNumber }} /></Table.Cell>
                        </Table.Row>
                      </Table.Body>
                    </Table>
                    <Divider horizontal>
                      <Header as='h4'>
                        <Icon name='hashtag'></Icon> 其他信息
                      </Header>
                    </Divider>
                    <Table definition>
                      <Table.Body>
                        {
                          userInfo.otherInfo.map((v, i) =>
                            <Table.Row key={i}>
                              <Table.Cell textAlign='center' width={4}>{v.name}</Table.Cell>
                              <Table.Cell><Input onBlur={handleChange} name={v.key} fluid defaultValue={v.value} /></Table.Cell>
                            </Table.Row>
                          )
                        }
                      </Table.Body>
                    </Table>
                  </Form>
                </Grid.Column>

                <Grid.Column>
                  <Divider horizontal>
                    <Header as='h4'>
                      <Icon name='pencil'></Icon>
                      做题记录
                    </Header>
                  </Divider>
                  {renderProblems()}
                </Grid.Column>
              </Grid>
            </Item.Description>
          </Item.Content>
        </Item>
      </Item.Group>
    </>;
  }

  let notSignedIn =
    <>
      <Header as='h1'>出现错误</Header>
      <Header as='h4' color='red'>请先登录账户</Header>
    </>;

  let loading = <>
    <Placeholder>
      <Placeholder.Header image>
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Header>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>
  </>;

  React.useEffect(() => {
    setTitle('门户');
    loadProblems();
  }, []);

  return userInfo.userId ? ((!userInfo.signedIn) ? notSignedIn : showUserInfo()) : loading;
};

export default User;