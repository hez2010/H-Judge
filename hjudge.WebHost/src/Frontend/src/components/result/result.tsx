import * as React from 'reactn';
import { Header, Item, Popup, Button, Segment, Grid, Label, Loader, Responsive, Placeholder } from 'semantic-ui-react';
import { NavLink } from 'react-router-dom';
import MarkdownViewer from '../viewer/markdown';
import { Get, Post } from '../../utils/requestHelper';
import { tryJson } from '../../utils/responseHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { getWidth } from '../../utils/windowHelper';
// import { HubConnectionBuilder, HubConnectionState, HubConnection } from '@aspnet/signalr';

interface JudgePointModel {
  score: number,
  timeCost: number,
  memoryCost: number,
  exitCode: number,
  extraInfo: string,
  resultType: number,
  result: string,
}

interface JudgeResultModel {
  judgePoints: JudgePointModel[]
  compileLog: string,
  staticCheckLog: string
}

interface ResultModel {
  resultId: number,
  userId: string,
  userName: string,
  problemId: number,
  problemName: string,
  contestId: number,
  contestName: string,
  groupId: number,
  groupName: string,
  resultType: number,
  result: string,
  content: string,
  type: number,
  time: Date,
  resultDisplayType: number,
  judgeResult: JudgeResultModel
}

interface ResultState {
  loaded: boolean,
  result: ResultModel
}

export default class Result extends React.Component<CommonProps, ResultState, GlobalState> {
  constructor() {
    super();

    this.state = {
      loaded: false,
      result: {
        resultId: 0,
        userId: '',
        userName: '',
        problemId: 0,
        problemName: '',
        contestId: 0,
        contestName: '',
        groupId: 0,
        groupName: '',
        resultType: 0,
        result: '',
        content: '',
        type: 0,
        time: new Date(),
        resultDisplayType: 0,
        judgeResult: {
          judgePoints: [],
          compileLog: '',
          staticCheckLog: ''
        }
      }
    };

    this.compileLogs = this.compileLogs.bind(this);
    this.judgeDetails = this.judgeDetails.bind(this);
    this.judgePoints = this.judgePoints.bind(this);
    this.loadResult = this.loadResult.bind(this);
    this.reJudge = this.reJudge.bind(this);
    this.renderSubmissionInfo = this.renderSubmissionInfo.bind(this);
    this.staticCheckLogs = this.staticCheckLogs.bind(this);
  }

  // private connection: HubConnection = new HubConnectionBuilder()
  //   .withUrl('/hub/judge')
  //   .build();

  loadResult(resultId: number) {
    Get(`/judge/result?id=${resultId}`)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ResultModel;
        result.time = new Date(result.time);
        this.setState({
          result: result,
          loaded: true
        });
        
        // TODO: remove it once bug in signalr was fixed
        if (result.resultType < 1) setTimeout(this.loadResult, 3000);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '评测结果加载失败', 'red');
        console.log(err);
      })
  };

  reJudge() {
    Post('/judge/rejudge', {
      resultId: this.state.result.resultId
    })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('成功', '重新评测请求成功', 'green');
        this.setState({ loaded: false });
        this.loadResult(parseInt(this.props.match.params.resultId));
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '重新评测请求失败', 'red');
        console.log(err);
      })
  }

  renderSubmissionInfo() {
    return <div>
      <small>提交时间：{this.state.result.time.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>提交用户：<NavLink to={`/user/${this.state.result.userId}`}>{this.state.result.userName}</NavLink></small>
      <br />
      <small>题目名称：<NavLink to={`/details/problem/${this.state.result.problemId}${!this.state.result.contestId ? '' : `/${this.state.result.contestId}`}${!this.state.result.groupId ? '' : `/${this.state.result.groupId}`}`}>{this.state.result.problemName}</NavLink></small>
      <br />
      {
        this.state.result.contestId ? <>
          <small>比赛名称：<NavLink to={`/details/contest/${this.state.result.contestId}${!this.state.result.groupId ? '' : `/${this.state.result.groupId}`}`}>{this.state.result.contestName}</NavLink></small>
          <br />
        </> : null
      }
      {
        this.state.result.groupId ? <>
          <small>小组名称：<NavLink to={`/details/group/${this.state.result.groupId}`}>{this.state.result.groupName}</NavLink></small>
          <br />
        </> : null
      }
      {
        this.state.result.resultType >= 1 && this.state.result.resultDisplayType === 0 && this.state.result.judgeResult.judgePoints.length !== 0 ? <>
          <small>所得分数：{this.state.result.judgeResult.judgePoints.map(v => v.score).reduce((accu, next) => accu + next)}</small>
          <br />
        </> : null
      }
    </div>;
  }

  judgePoints() {
    return this.state.result.judgeResult.judgePoints.map((v, i) =>
      <Grid.Column key={i}>
        <Segment>
          <Label ribbon color='black'>#{i + 1}</Label>
          <Header as='span' color={v.resultType < 1 ? undefined : (v.resultType === 1 ? 'green' : 'red')}>{v.result}</Header>
          <br />
          <small style={{ wordBreak: 'break-all', wordWrap: 'break-word' }}>时间：{v.timeCost}ms</small>
          <br />
          <small style={{ wordBreak: 'break-all', wordWrap: 'break-word' }}>内存：{v.memoryCost}ms</small>
          <br />
          <small style={{ wordBreak: 'break-all', wordWrap: 'break-word' }}>退出代码：{v.exitCode}</small>
          <br />
          <small style={{ wordBreak: 'break-all', wordWrap: 'break-word' }}>
            <div style={{ alignItems: 'flex-end', display: 'flex', height: '5em' }}>
              <pre style={{ width: '100%', maxHeight: '4em', overflow: 'auto', scrollBehavior: 'auto' }}>{!!v.extraInfo ? v.extraInfo : '其他信息：无'}</pre>
            </div>
          </small>
        </Segment>
      </Grid.Column>
    )
  };

  judgeDetails() {
    return this.state.result.judgeResult.judgePoints.length === 0 ? null : <>
      <Header as='h2'>评测详情</Header>
      <Responsive getWidth={getWidth} minWidth={Responsive.onlyTablet.minWidth}>
        <Grid columns={3}>
          {this.judgePoints()}
        </Grid>
      </Responsive>
      <Responsive getWidth={getWidth} maxWidth={Responsive.onlyMobile.maxWidth}>
        <Grid columns={1}>
          {this.judgePoints()}
        </Grid>
      </Responsive>
    </>;
  }

  compileLogs() {
    return !!this.state.result.judgeResult.compileLog ? <>
      <Header as='h2'>编译日志</Header>
      <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
        <MarkdownViewer content={'```\n' + this.state.result.judgeResult.compileLog + '\n```'} />
      </div>
    </> : null;
  }

  staticCheckLogs() {
    return !!this.state.result.judgeResult.staticCheckLog ? <>
      <Header as='h2'>静态检查日志</Header>
      <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
        <MarkdownViewer content={'```\n' + this.state.result.judgeResult.staticCheckLog + '\n```'} />
      </div>
    </> : null;
  }

  componentDidMount() {
    setTitle('评测结果');
    
    // TODO: re-enable it once bug in signalr was fixed
    // this.connection.on('JudgeCompleteSignalReceived', (resultId: number) => {
    //   this.loadResult(resultId);
    // });

    // this.connection.start()
    //   .then(() => this.connection.invoke("SubscribeJudgeResult", parseInt(this.props.match.params.resultId))
    //     .then(() => this.loadResult(parseInt(this.props.match.params.resultId)))
    //     .catch((err: any) => {
    //       this.loadResult(parseInt(this.props.match.params.resultId));
    //       console.log(err);
    //     }))
    //   .catch((err: any) => console.log(err));

    this.loadResult(parseInt(this.props.match.params.resultId))
  }

  componentWillUnmount() {
    // if (this.connection.state === HubConnectionState.Connected) this.connection.stop();
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

    if (this.state.result.resultType < 1) return <Item>
      <Item.Content>
        <Item.Header as='h2'>
          <Popup flowing hoverable position='right center' trigger={<span>#{this.state.result.resultId}：<span style={{ color: `${this.state.result.resultType < 1 ? 'black' : (this.state.result.resultType === 1 ? 'green' : 'red')}` }}>{this.state.result.result}</span></span>}>
            <Popup.Header>评测信息</Popup.Header>
            <Popup.Content>{this.renderSubmissionInfo()}</Popup.Content>
          </Popup>
        </Item.Header>

        <Item.Description>
          {
            !!this.state.result.content ? <><Header as='h2'>提交内容</Header>
              <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
                <MarkdownViewer content={'```\n' + this.state.result.content + '\n```'} />
              </div></> : null
          }
          <Loader active inline>评测中...</Loader>
        </Item.Description>
      </Item.Content>
    </Item>;

    return <>
      <Item>
        <Item.Content>
          <Item.Header as='h2'>
            <Popup flowing hoverable position='right center' trigger={<span>#{this.state.result.resultId}：<span style={{ color: `${this.state.result.resultType < 1 ? 'black' : (this.state.result.resultType === 1 ? 'green' : 'red')}` }}>{this.state.result.result}</span></span>}>
              <Popup.Header>评测信息</Popup.Header>
              <Popup.Content>{this.renderSubmissionInfo()}</Popup.Content>
            </Popup>
            <div style={{ float: 'right' }}>
              <Button.Group>
                <Button onClick={this.reJudge}>重新评测</Button>
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            {
              !!this.state.result.content ? <><Header as='h2'>提交内容</Header>
                <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
                  <MarkdownViewer content={'```\n' + this.state.result.content + '\n```'} />
                </div></> : null
            }
          </Item.Description>
        </Item.Content>
      </Item>

      {
        this.state.result.resultDisplayType === 0 ?
          <>
            {this.judgeDetails()}
            {this.compileLogs()}
            {this.staticCheckLogs()}
          </> : null
      }
    </>;
  }
}