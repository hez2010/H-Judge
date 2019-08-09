import * as React from 'react';
import { Item, Popup, Input, Divider, Header, Icon, Table, Label, Form, Placeholder, Grid } from 'semantic-ui-react';
import { OtherInfo } from '../../interfaces/userInfo';
import { setTitle } from '../../utils/titleHelper';
import { Post, Get } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { CommonProps } from '../../interfaces/commonProps';
import { NavLink } from 'react-router-dom';

interface UserProps extends CommonProps { }

interface UserState {
  statistics: ProblemStatisticsModel
}

interface ProblemStatisticsModel extends ResultModel {
  solvedProblems: number[],
  triedProblems: number[],
  loaded: boolean
}

export default class User extends React.Component<UserProps, UserState> {
  constructor(props: UserProps) {
    super(props);
    this.confirmEmail = this.confirmEmail.bind(this);
    this.confirmPhoneNumber = this.confirmPhoneNumber.bind(this);
    this.changeAvatar = this.changeAvatar.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.loadProblems = this.loadProblems.bind(this);
    this.renderProblems = this.renderProblems.bind(this);

    this.state = {
      statistics: {
        solvedProblems: [],
        triedProblems: [],
        loaded: false
      }
    };
  }

  componentDidMount() {
    setTitle('门户');
    this.loadProblems();
  }

  confirmEmail(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    this.props.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  confirmPhoneNumber(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    this.props.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
  }

  changeAvatar(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    element.disabled = true;
    this.props.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
    element.disabled = false;
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
      .then(res => res.json())
      .then(data => {
        let result = data as ResultModel;
        if (result.succeeded) {
          this.props.openPortal('提示', '信息更新成功', 'green');
          this.props.refreshUserInfo();
        }
        else {
          this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '信息更新失败', 'red');
        console.log(err);
      });
  }

  loadProblems() {
    Get('/user/stats')
      .then(res => res.json())
      .then(data => {
        let result = data as ProblemStatisticsModel;
        if (result.succeeded) {
          this.setState({
            statistics: {
              solvedProblems: result.solvedProblems,
              triedProblems: result.triedProblems,
              loaded: true
            }
          });
        }
        else {
          this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '做题记录加载失败', 'red');
        console.log(err);
      });
  }

  renderProblems() {
    if (!this.state.statistics.loaded) {
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
    let solved = this.state.statistics.solvedProblems.length === 0 ? <p>无</p> :
      <Grid columns={10}>{this.state.statistics.solvedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;
    let tried = this.state.statistics.triedProblems.length === 0 ? <p>无</p> :
      <Grid columns={10}>{this.state.statistics.triedProblems.map((v, i) =>
        <Grid.Column key={i}><NavLink to={`/details/problem/${v}`} >#{v}</NavLink></Grid.Column>
      )}</Grid>;

    return <>
      <Header as='h5'>已通过的题目</Header>
      {solved}
      <Header as='h5'>已尝试的题目</Header>
      {tried}
    </>;
  }

  showUserInfo() {
    return <>
      <Item.Group>
        <Item>
          <div>
            <Popup
              position='bottom center'
              trigger={<Item.Image size='small' src={`/user/avatar?userId=${this.props.userInfo.userId}`} circular style={{ cursor: 'pointer' }} onClick={this.changeAvatar} />}
              content='点击更换头像'
            />
          </div>
          <Item.Content>
            <Item.Header>{this.props.userInfo.userName}</Item.Header>
            <Item.Meta><Label>{this.props.userInfo.privilege === 1 ? '管理员' :
              this.props.userInfo.privilege === 2 ? '教师' :
                this.props.userInfo.privilege === 3 ? '助教' :
                  this.props.userInfo.privilege === 4 ? '学生' :
                    this.props.userInfo.privilege === 5 ? '黑名单' : '未知'}</Label> 经验：{this.props.userInfo.experience}，金币：{this.props.userInfo.coins}</Item.Meta>
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
                          <Table.Cell><Input name='name' onBlur={this.handleChange} fluid defaultValue={this.props.userInfo.name} /></Table.Cell>
                        </Table.Row>
                        <Table.Row>
                          <Table.Cell textAlign='center' width={4}>邮箱</Table.Cell>
                          <Table.Cell><Input name='email' onBlur={this.handleChange} fluid defaultValue={this.props.userInfo.email} type='email' action={this.props.userInfo.emailConfirmed ? null : { primary: true, content: '验证', onClick: this.confirmEmail }} /></Table.Cell>
                        </Table.Row>
                        <Table.Row>
                          <Table.Cell textAlign='center' width={4}>手机</Table.Cell>
                          <Table.Cell><Input name='phoneNumber' onBlur={this.handleChange} fluid defaultValue={this.props.userInfo.phoneNumber} action={this.props.userInfo.phoneNumberConfirmed ? null : { primary: true, content: '验证', onClick: this.confirmPhoneNumber }} /></Table.Cell>
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
                          this.props.userInfo.otherInfo.map((v, i) =>
                            <Table.Row key={i}>
                              <Table.Cell textAlign='center' width={4}>{v.name}</Table.Cell>
                              <Table.Cell><Input onBlur={this.handleChange} name={v.key} fluid defaultValue={v.value} /></Table.Cell>
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

  render() {
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
    </>
    return this.props.userInfo.succeeded ? ((!this.props.userInfo.signedIn) ? notSignedIn : this.showUserInfo()) : loading;
  }
}