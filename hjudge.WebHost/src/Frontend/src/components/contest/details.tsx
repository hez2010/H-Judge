import * as React from 'reactn';
import { CommonProps } from '../../interfaces/commonProps';
import { Item, Popup, Button, Rating, Placeholder, Divider, Header, Icon, Progress, Form, Label } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';
import { isTeacher } from '../../utils/privilegeHelper';
import { NavLink } from 'react-router-dom';
import Problem from '../problem/problem';
import { Post } from '../../utils/requestHelper';
import { setTitle } from '../../utils/titleHelper';
import { GlobalState } from '../../interfaces/globalState';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';

interface ContestDetailsProps extends CommonProps {
  contestId?: number,
  groupId?: number
}

enum ContestType {
  generic, lastSubmit, penalty
}

enum ResultDisplayMode {
  intime, afterContest, never
}

enum ResultDisplayType {
  detailed, summary
}

interface ContestConfig {
  type: ContestType,
  submissionLimit: number,
  resultMode: ResultDisplayMode,
  resultType: ResultDisplayType,
  showRank: boolean,
  autoStopRank: boolean,
  languages: string,
  canMakeResultPublic: boolean,
  canDiscussion: boolean
}

export interface ContestModel {
  id: number,
  name: string,
  startTime: Date,
  endTime: Date,
  password: string,
  userId: string,
  userName: string,
  description: string,
  config: ContestConfig,
  hidden: boolean,
  upvote: number,
  downvote: number,
  currentTime: Date,
  myVote: number
}

interface ContestDetailsState {
  contest: ContestModel,
  progress: number,
  status: number,
  inputPassword: string,
  loaded: boolean
}

export default class ContestDetails extends React.Component<ContestDetailsProps, ContestDetailsState, GlobalState> {
  constructor() {
    super();
    this.state = {
      contest: {
        id: 0,
        startTime: new Date(),
        endTime: new Date(),
        currentTime: new Date(Date.now()),
        config: {
          type: ContestType.generic,
          autoStopRank: false,
          canDiscussion: false,
          canMakeResultPublic: false,
          languages: '',
          resultMode: ResultDisplayMode.intime,
          resultType: ResultDisplayType.detailed,
          showRank: true,
          submissionLimit: 0
        },
        description: '',
        downvote: 0,
        upvote: 0,
        hidden: false,
        name: '',
        password: '',
        userId: '',
        userName: '',
        myVote: 0
      },
      progress: 0,
      status: 0,
      inputPassword: '',
      loaded: false
    }

    this.editContest = this.editContest.bind(this);
    this.renderContestInfo = this.renderContestInfo.bind(this);
    this.fetchDetail = this.fetchDetail.bind(this);
    this.updateProgressBar = this.updateProgressBar.bind(this);
    this.problemList = this.problemList.bind(this);
    this.voteContest = this.voteContest.bind(this);
  }

  private contestId: number = 0;
  private groupId: number = 0;
  private currentTime: number = 0;
  private timer?: NodeJS.Timeout;

  updateProgressBar() {
    let offset = this.currentTime - this.state.contest.currentTime.getTime();
    let curTime = Date.now() - offset;
    let start = this.state.contest.startTime.getTime();
    let end = this.state.contest.endTime.getTime();

    let progress = Math.floor((curTime - start) * 10000.0 / (end - start));
    if (progress < 0) progress = 0;
    if (progress > 10000) progress = 10000;

    let status = 0;

    if (curTime <= start) status = 0;
    else if (curTime >= end) status = 2;
    else status = 1;

    this.setState({
      progress: progress,
      status: status
    } as ContestDetailsState);
  }

  componentDidMount() {
    setTitle('比赛详情');

    if (this.props.contestId) this.contestId = this.props.contestId;
    else if (this.props.match.params.contestId) this.contestId = parseInt(this.props.match.params.contestId)
    else this.contestId = 0;

    if (this.props.groupId) this.groupId = this.props.groupId;
    else if (this.props.match.params.groupId) this.groupId = parseInt(this.props.match.params.groupId)
    else this.groupId = 0;

    this.fetchDetail(this.contestId, this.groupId);
  }

  fetchDetail(contestId: number, groupId: number) {
    Post('/contest/details', {
      contestId: contestId,
      groupId: groupId
    })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ContestModel;
        result.startTime = new Date(result.startTime.toString());
        result.endTime = new Date(result.endTime.toString());
        result.currentTime = new Date(result.currentTime.toString());

        this.setState({
          contest: result,
          inputPassword: window.sessionStorage.getItem(`contest_${this.contestId}`),
          loaded: true
        } as ContestDetailsState);
        setTitle(result.name);
        this.currentTime = Date.now();
        this.timer = setInterval(this.updateProgressBar, 1000);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '比赛信息加载失败', 'red');
        console.log(err);
      });
  }

  componentWillUnmount() {
    if (this.timer) clearInterval(this.timer);
    this.timer = undefined;
    if (this.state.inputPassword) {
      window.sessionStorage.setItem(`contest_${this.contestId}`, this.state.inputPassword);
    }
  }

  renderContestInfo() {
    return <div>
      <small>创建用户：<NavLink to={`/user/${this.state.contest.userId}`}>{this.state.contest.userName}</NavLink></small>
      <br />
      <small>比赛类型：{this.state.contest.config.type === ContestType.generic ? '一般计时赛' : this.state.contest.config.type === ContestType.lastSubmit ? '最后提交赛' : '罚时计时赛'}</small>
      <br />
      <small>提交限制：{this.state.contest.config.submissionLimit === 0 ? '无限次' : this.state.contest.config.submissionLimit}</small>
      <br />
      <small>比赛评分：{this.state.contest.upvote + this.state.contest.downvote === 0 ? <Rating icon='star' defaultRating={3} maxRating={5} disabled={true} /> : <Rating icon='star' defaultRating={3} maxRating={5} disabled={true} rating={Math.round(this.state.contest.upvote * 5 / (this.state.contest.upvote + this.state.contest.downvote))} />}</small>
    </div>;
  }

  editContest(id: number) {
    this.props.history.push(`/edit/contest/${id}`);
  }

  voteContest(voteType: number) {
    if (this.state.contest.myVote !== 0) {
      Post('/vote/cancel', {
        contestId: this.state.contest.id
      })
        .then(res => tryJson(res))
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          this.global.commonFuncs.openPortal('成功', '评价取消成功', 'green');
          let contest = { ...this.state.contest };
          contest.myVote = 0;
          this.setState({ contest: contest });
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '评价取消失败', 'red');
          console.log(err);
        })
      return;
    }
    
    Post('/vote/contest', {
      contestId: this.state.contest.id,
      voteType: voteType
    })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('成功', '评价成功', 'green');
        let contest = { ...this.state.contest };
        contest.myVote = voteType;
        this.setState({ contest: contest });
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '评价失败', 'red');
        console.log(err);
      })
  }

  problemList() {
    if (Date.now() < this.state.contest.startTime.getTime()) return <p>比赛未开始</p>;
    let authed = true;
    if (!!this.state.contest.password && this.state.inputPassword !== this.state.contest.password) {
      authed = false;
    }

    let props = { ...this.props };
    props.contestId = this.contestId;

    return authed ? <Problem {...props} /> :
      <Form>
        <Form.Field>
          <label>比赛密码</label>
          <Form.Input type='password' defaultValue={this.state.inputPassword} error={this.state.inputPassword !== this.state.contest.password} onChange={(_, data) => { this.setState({ inputPassword: data.value }) }} />
        </Form.Field>
      </Form>;
  }

  render() {
    const placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;
    if (!this.state.loaded) return placeHolder;

    return <>
      <Item>
        <Item.Content>
          <Item.Header as='h2'>
            <Popup flowing hoverable position='right center' trigger={<span>{this.state.contest.name}</span>}>
              <Popup.Header>比赛信息</Popup.Header>
              <Popup.Content>{this.renderContestInfo()}</Popup.Content>
            </Popup>
            <div style={{ float: 'right' }}>
              <Button.Group>
                <Button icon onClick={() => this.voteContest(1)}>
                  <Icon name='thumbs up' color={this.state.contest.myVote === 1 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button icon onClick={() => this.voteContest(2)}>
                  <Icon name='thumbs down' color={this.state.contest.myVote === 2 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button onClick={() => this.props.history.push(`/statistics/-1/${this.groupId ? `${this.groupId}` : '-1'}/${this.contestId}/0/-1`)}>状态</Button>
                <Button>排名</Button>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button primary onClick={() => this.editContest(this.state.contest.id)}>编辑</Button> : null}
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            <br />
            <Progress active={this.state.status === 1} error={this.state.status === 2} success={this.state.status === 0} warning={this.state.status === 1} percent={this.state.progress / 100} inverted progress>{this.state.status === 0 ? '未开始' : this.state.status === 1 ? '进行中' : '已结束'}（{this.state.contest.startTime.toLocaleString(undefined, { hour12: false })} - {this.state.contest.endTime.toLocaleString(undefined, { hour12: false })}）</Progress>

            <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%' }}>
              <MarkdownViewer content={this.state.contest.description} />
            </div>
          </Item.Description>
          <Item.Extra>
          </Item.Extra>
        </Item.Content>
      </Item>
      <Divider horizontal>
        <Header as='h4'>
          <Icon name='list' />题目列表
      </Header>
      </Divider>
      {this.problemList()}
    </>;
  }
}