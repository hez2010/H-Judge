import * as React from 'react';
import { useGlobal } from 'reactn';
import { Header, Placeholder, Item, Popup, Button, Card, Segment, Grid, Label, Loader, Responsive } from 'semantic-ui-react';
import { NavLink } from 'react-router-dom';
import MarkdownViewer from '../viewer/markdown';
import { Get, Post } from '../../utils/requestHelper';
import { tryJson } from '../../utils/responseHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { GlobalState } from '../../interfaces/globalState';
import { getTargetState } from '../../utils/reactnHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { getWidth } from '../../utils/windowHelper';
import * as SignalR from '@aspnet/signalr';

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

let connection: SignalR.HubConnection;

const Result = (props: CommonProps) => {
  const [loaded, setLoaded] = React.useState<boolean>(false);
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  const [result, setResult] = React.useState<ResultModel>({
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
  });

  const loadResult = (resultId: number) => {
    Get(`/judge/result?id=${resultId}`)
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ResultModel;
        result.time = new Date(result.time);
        setResult(result);
        setLoaded(true);
      })
      .catch(err => {
        commonFuncs.openPortal('错误', '评测结果加载失败', 'red');
        console.log(err);
      })
  };

  const reJudge = () => {
    Post('/judge/rejudge', {
      resultId: result.resultId
    })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        commonFuncs.openPortal('成功', '重新评测请求成功', 'green');
        setLoaded(false);
        loadResult(parseInt(props.match.params.resultId));
      })
      .catch(err => {
        commonFuncs.openPortal('错误', '重新评测请求失败', 'red');
        console.log(err);
      })
  }

  const renderSubmissionInfo = () => {
    return <div>
      <small>提交时间：{result.time.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>提交用户：<NavLink to={`/user/${result.userId}`}>{result.userName}</NavLink></small>
      <br />
      <small>题目名称：<NavLink to={`/details/problem/${result.problemId}${!result.contestId ? '' : `/${result.contestId}`}${!result.groupId ? '' : `/${result.groupId}`}`}>{result.problemName}</NavLink></small>
      <br />
      {
        result.contestId ? <>
          <small>比赛名称：<NavLink to={`/details/contest/${result.contestId}${!result.groupId ? '' : `/${result.groupId}`}`}>{result.contestName}</NavLink></small>
          <br />
        </> : null
      }
      {
        result.groupId ? <>
          <small>小组名称：<NavLink to={`/details/group/${result.groupId}`}>{result.groupName}</NavLink></small>
          <br />
        </> : null
      }
      {
        result.resultType >= 1 && result.resultDisplayType === 0 && result.judgeResult.judgePoints.length !== 0 ? <>
          <small>所得分数：{result.judgeResult.judgePoints.map(v => v.score).reduce((accu, next) => accu + next)}</small>
          <br />
        </> : null
      }
    </div>;
  }

  const judgePoints = () => result.judgeResult.judgePoints.map((v, i) =>
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
  );

  const judgeDetails = () => result.judgeResult.judgePoints.length === 0 ? null : <>
    <Header as='h2'>评测详情</Header>
    <Responsive getWidth={getWidth} minWidth={Responsive.onlyTablet.minWidth}>
      <Grid columns={3}>
        {judgePoints()}
      </Grid>
    </Responsive>
    <Responsive getWidth={getWidth} maxWidth={Responsive.onlyMobile.maxWidth}>
      <Grid columns={1}>
        {judgePoints()}
      </Grid>
    </Responsive>
  </>;

  const compileLogs = () => !!result.judgeResult.compileLog ? <>
    <Header as='h2'>编译日志</Header>
    <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
      <MarkdownViewer content={'```\n' + result.judgeResult.compileLog + '\n```'} />
    </div>
  </> : null;

  const staticCheckLogs = () => !!result.judgeResult.staticCheckLog ? <>
    <Header as='h2'>静态检查日志</Header>
    <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
      <MarkdownViewer content={'```\n' + result.judgeResult.staticCheckLog + '\n```'} />
    </div>
  </> : null;

  React.useEffect(() => {
    setTitle('评测结果');
    connection = new SignalR.HubConnectionBuilder()
      .withUrl('/hub/judge')
      .build();

    connection.on('JudgeCompleteSignalReceived', (resultId: number) => {
      loadResult(resultId);
    });

    connection.start()
      .then(() => connection.invoke("SubscribeJudgeResult", parseInt(props.match.params.resultId))
        .then(() => loadResult(parseInt(props.match.params.resultId)))
        .catch(err => {
          loadResult(parseInt(props.match.params.resultId));
          console.log(err);
        }))
      .catch(err => console.log(err));

    return () => {
      if (connection.state === SignalR.HubConnectionState.Connected) connection.stop();
    }
  }, []);

  const placeHolder = <Placeholder>
    <Placeholder.Paragraph>
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
    </Placeholder.Paragraph>
  </Placeholder>;
  if (!loaded) return placeHolder;

  if (result.resultType < 1) return <Item>
    <Item.Content>
      <Item.Header as='h2'>
        <Popup flowing hoverable position='right center' trigger={<span>#{result.resultId}：<span style={{ color: `${result.resultType < 1 ? 'black' : (result.resultType === 1 ? 'green' : 'red')}` }}>{result.result}</span></span>}>
          <Popup.Header>评测信息</Popup.Header>
          <Popup.Content>{renderSubmissionInfo()}</Popup.Content>
        </Popup>
      </Item.Header>

      <Item.Description>
        {
          !!result.content ? <><Header as='h2'>提交内容</Header>
            <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
              <MarkdownViewer content={'```\n' + result.content + '\n```'} />
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
          <Popup flowing hoverable position='right center' trigger={<span>#{result.resultId}：<span style={{ color: `${result.resultType < 1 ? 'black' : (result.resultType === 1 ? 'green' : 'red')}` }}>{result.result}</span></span>}>
            <Popup.Header>评测信息</Popup.Header>
            <Popup.Content>{renderSubmissionInfo()}</Popup.Content>
          </Popup>
          <div style={{ float: 'right' }}>
            <Button.Group>
              <Button onClick={reJudge}>重新评测</Button>
            </Button.Group>
          </div>
        </Item.Header>

        <Item.Description>
          {
            !!result.content ? <><Header as='h2'>提交内容</Header>
              <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%', maxHeight: '30em' }}>
                <MarkdownViewer content={'```\n' + result.content + '\n```'} />
              </div></> : null
          }
        </Item.Description>
      </Item.Content>
    </Item>

    {
      result.resultDisplayType === 0 ?
        <>
          {judgeDetails()}
          {compileLogs()}
          {staticCheckLogs()}
        </> : null
    }
  </>;
};

export default Result;