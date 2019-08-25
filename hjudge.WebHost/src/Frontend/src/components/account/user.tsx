import { Item, Popup, Input, Divider, Header, Icon, Table, Label, Form, Placeholder, Grid } from 'semantic-ui-react';
import { OtherInfo, UserInfo } from '../../interfaces/userInfo';
import { setTitle } from '../../utils/titleHelper';
import { Post, Get, Put } from '../../utils/requestHelper';
import { NavLink } from 'react-router-dom';
import * as React from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';

interface ProblemStatisticsModel {
  solvedProblems: number[],
  triedProblems: number[],
  loaded: boolean
}

interface OtherUserModel {
  otherUser: boolean
}

interface UserState {
  userInfo: UserInfo & OtherUserModel,
  solvedProblems: number[],
  triedProblems: number[],
  loaded: boolean,
  cnt: number
}

export default class User extends React.Component<CommonProps, UserState, GlobalState> {
  constructor() {
    super();
    this.state = {
      userInfo: {
        coins: 0,
        email: '',
        emailConfirmed: false,
        experience: 0,
        name: '',
        otherInfo: [],
        phoneNumber: '',
        phoneNumberConfirmed: false,
        privilege: 4,
        signedIn: false,
        userId: '',
        userName: '',
        otherUser: false
      },
      solvedProblems: [],
      triedProblems: [],
      loaded: false,
      cnt: 0
    };

    this.changeAvatar = this.changeAvatar.bind(this);
    this.confirmEmail = this.confirmEmail.bind(this);
    this.confirmPhoneNumber = this.confirmPhoneNumber.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.loadUser = this.loadUser.bind(this);
    this.loadProblems = this.loadProblems.bind(this);
    this.renderProblems = this.renderProblems.bind(this);
    this.showUserInfo = this.showUserInfo.bind(this);
    this.uploadFile = this.uploadFile.bind(this);
  }

  private fileLoader = React.createRef<HTMLInputElement>();

  uploadFile() {
    let ele = this.fileLoader.current;
    if (!ele || !ele.files || ele.files.length === 0) return;
    let file = ele.files[0];
    if (!file.type.startsWith('image/')) {
      this.global.commonFuncs.openPortal('错误', '文件格式不正确', 'red');
      ele.value = '';
      return;
    }
    if (file.size > 1048576) {
      this.global.commonFuncs.openPortal('错误', '文件大小不能超过 1 Mb', 'red');
      ele.value = '';
      return;
    }
    let form = new FormData();
    form.append('avatar', file);
    Put('/user/avatar', form, false, '')
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        else {
          this.global.commonFuncs.openPortal('成功', '头像上传成功', 'green');
          this.setState({ cnt: this.state.cnt + 1 });
        }
        let ele = this.fileLoader.current;
        if (ele) ele.value = '';
      })
      .catch(() => {
        this.global.commonFuncs.openPortal('错误', '头像上传失败', 'red');
        let ele = this.fileLoader.current;
        if (ele) ele.value = '';
      });
  }

  confirmEmail(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    this.global.commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  confirmPhoneNumber(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    this.global.commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  changeAvatar(_event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    if (this.fileLoader.current) {
      this.fileLoader.current.click();
    }
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
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
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('提示', '信息更新成功', 'green');
        this.global.commonFuncs.refreshUserInfo();
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '信息更新失败', 'red');
        console.log(err);
      });
  }

  loadUser(userId?: string) {
    Get(`/user/profiles${userId ? `?userId=${userId}` : ''}`)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as UserInfo & OtherUserModel;
        result.otherUser = !!userId;
        this.setState({ userInfo: result });
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '用户信息加载失败', 'red');
        console.log(err);
      });
  }

  loadProblems(userId?: string) {
    Get(`/user/stats${userId ? `?userId=${userId}` : ''}`)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemStatisticsModel;
        this.setState({ solvedProblems: result.solvedProblems, triedProblems: result.triedProblems, loaded: true });
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '做题记录加载失败', 'red');
        console.log(err);
      });
  }

  renderProblems() {
    if (!this.state.loaded) {
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

    let solved = this.state.solvedProblems.length === 0 ? <p>无</p> :
      <Grid columns={8}>{this.state.solvedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;
    let tried = this.state.triedProblems.length === 0 ? <p>无</p> :
      <Grid columns={8}>{this.state.triedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;
    return <>
      <Header as='h5'>已通过的题目</Header>
      {solved}
      <Header as='h5'>已尝试的题目</Header>
      {tried}
    </>;
  }

  showUserInfo(otherUser: boolean) {
    return <>
      <Item.Group>
        <Item>
          <div>
            {
              otherUser ? <Item.Image size='small' src={`/user/avatar?userId=${this.state.userInfo.userId}`} circular /> :
                <>
                  <Popup
                    position='bottom center'
                    trigger={<Item.Image size='small' src={`/user/avatar?userId=${this.state.userInfo.userId}&cnt=${this.state.cnt}`} circular style={{ cursor: 'pointer' }} onClick={this.changeAvatar} />}
                    content='点击更换头像'
                  />
                  <input ref={this.fileLoader} onChange={this.uploadFile} type='file' accept="image/png,image/gif,image/jpg,image/jpeg,image/tiff,image/bmp" style={{ filter: 'alpha(opacity=0)', opacity: 0, width: 0, height: 0 }} />
                </>
            }
          </div>
          <Item.Content>
            <Item.Header>{this.state.userInfo.userName}<Label>编号：{this.state.userInfo.userId}</Label></Item.Header>
            <Item.Meta><Label>{this.state.userInfo.privilege === 1 ? '管理员' :
              this.state.userInfo.privilege === 2 ? '教师' :
                this.state.userInfo.privilege === 3 ? '助教' :
                  this.state.userInfo.privilege === 4 ? '学生' :
                    this.state.userInfo.privilege === 5 ? '黑名单' : '未知'}</Label> 经验：{this.state.userInfo.experience}，金币：{this.state.userInfo.coins}</Item.Meta>
            <Item.Description>

              <Grid columns={2} relaxed='very' stackable>
                <Grid.Column>
                  <Form id='infoForm'>
                    {
                      otherUser ? (!!this.state.userInfo.name ?
                        <><Divider horizontal>
                          <Header as='h4'><Icon name='tag'></Icon>基本信息</Header>
                        </Divider>
                          <Table definition>
                            <Table.Body>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>姓名</Table.Cell>
                                <Table.Cell><p>{this.state.userInfo.name}</p></Table.Cell>
                              </Table.Row>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>邮箱</Table.Cell>
                                <Table.Cell><p style={{ color: `${this.state.userInfo.emailConfirmed ? 'green' : 'red'}` }}>{this.state.userInfo.email}</p></Table.Cell>
                              </Table.Row>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>手机</Table.Cell>
                                <Table.Cell><p style={{ color: `${this.state.userInfo.phoneNumberConfirmed ? 'green' : 'red'}` }}>{this.state.userInfo.phoneNumber}</p></Table.Cell>
                              </Table.Row>
                            </Table.Body>
                          </Table></>
                        : null) : <><Divider horizontal>
                          <Header as='h4'><Icon name='tag'></Icon>基本信息</Header>
                        </Divider>
                          <Table definition>
                            <Table.Body>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>姓名</Table.Cell>
                                <Table.Cell><Input name='name' onBlur={this.handleChange} fluid defaultValue={this.state.userInfo.name} /></Table.Cell>
                              </Table.Row>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>邮箱</Table.Cell>
                                <Table.Cell><Input name='email' onBlur={this.handleChange} fluid defaultValue={this.state.userInfo.email} type='email' action={this.state.userInfo.emailConfirmed ? null : { primary: true, content: '验证', onClick: this.confirmEmail }} /></Table.Cell>
                              </Table.Row>
                              <Table.Row>
                                <Table.Cell textAlign='center' width={4}>手机</Table.Cell>
                                <Table.Cell><Input name='phoneNumber' onBlur={this.handleChange} fluid defaultValue={this.state.userInfo.phoneNumber} action={this.state.userInfo.phoneNumberConfirmed ? null : { primary: true, content: '验证', onClick: this.confirmPhoneNumber }} /></Table.Cell>
                              </Table.Row>
                            </Table.Body>
                          </Table></>
                    }
                    <Divider horizontal>
                      <Header as='h4'>
                        <Icon name='hashtag'></Icon> 其他信息
                      </Header>
                    </Divider>
                    <Table definition>
                      <Table.Body>
                        {
                          this.state.userInfo.otherInfo.map((v, i) =>
                            <Table.Row key={i}>
                              <Table.Cell textAlign='center' width={4}>{v.name}</Table.Cell>
                              <Table.Cell>
                                {
                                  otherUser ? <p>{v.value}</p> : <Input onBlur={this.handleChange} name={v.key} fluid defaultValue={v.value} />
                                }
                              </Table.Cell>
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
                  {this.renderProblems()}
                </Grid.Column>
              </Grid>
            </Item.Description>
          </Item.Content>
        </Item>
      </Item.Group>
    </>;
  }

  componentDidMount() {
    setTitle('门户');
    this.loadUser(this.props.match.params.userId);
    this.loadProblems(this.props.match.params.userId);
  }

  render() {
    const notSignedIn =
      <>
        <Header as='h1'>出现错误</Header>
        <Header as='h4' color='red'>请先登录账户</Header>
      </>;

    const loading = <>
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

    if (!this.state.userInfo.userId) return loading;
    if (!this.state.userInfo.otherUser && !this.state.userInfo.signedIn) return notSignedIn;
    return this.showUserInfo(this.state.userInfo.otherUser);
  }
}