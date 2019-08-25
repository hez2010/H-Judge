import * as React from 'react';
import { Get } from '../../utils/requestHelper';
import { tryJson } from '../../utils/responseHelper';
import { Placeholder, Table } from 'semantic-ui-react';
import { ErrorModel } from '../../interfaces/errorModel';
import { useGlobal } from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { getTargetState } from '../../utils/reactnHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { NavLink } from 'react-router-dom';

interface RankProps {
  contestId: number,
  groupId: number
}

interface RankUserInfoModel {
  rank: number,
  userName: string,
  name: string,
  score: number,
  time: string,
  penalty: number
}

interface RankContestItemModel {
  accepted: boolean,
  time: string,
  penalty: number,
  score: number,
  acceptCount: number,
  submissionCount: number
}

interface RankProblemInfoModel {
  problemName: string,
  acceptCount: number,
  submissionCount: number
}

interface RankContestStatisticsModel {
  contestId: number,
  groupId: number,
  problemInfos: any,
  userInfos: any,
  rankInfos: any
}

const Rank = (props: RankProps) => {
  const [loaded, setLoaded] = React.useState<boolean>(false);
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));
  const [rankInfo, setRankInfo] = React.useState<RankContestStatisticsModel>({
    contestId: 0,
    groupId: 0,
    problemInfos: {},
    rankInfos: {},
    userInfos: {}
  });

  function* iterMap<T>(source: any): Iterable<[string, T]> {
    for (let i in source) yield [i, source[i] as T];
  }

  React.useEffect(() => {
    Get(`/rank/contest?contestId=${props.contestId ? props.contestId : 0}&groupId=${props.groupId ? props.groupId : 0}`)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as RankContestStatisticsModel;
        setRankInfo(result);
        setLoaded(true);
      })
  }, []);

  const rankTable = () => <div style={{ width: '100%', height: '100%', overflow: 'auto', scrollBehavior: 'auto' }}>
    <Table striped celled>
      <Table.Header>
        <Table.Row>
          <Table.HeaderCell textAlign='center'>排名</Table.HeaderCell>
          <Table.HeaderCell textAlign='center'>用户</Table.HeaderCell>
          <Table.HeaderCell textAlign='center'>总分</Table.HeaderCell>
          <Table.HeaderCell textAlign='center'>用时</Table.HeaderCell>
          {
            Array.from(iterMap<RankProblemInfoModel>(rankInfo.problemInfos)).map((v, i) =>
              <Table.HeaderCell textAlign='center' key={i}><NavLink to={`/details/problem/${v[0]}/${props.contestId}${props.groupId ? `/${props.groupId}` : ''}`}>{v[1].problemName}</NavLink><br />({v[1].acceptCount}/{v[1].submissionCount})</Table.HeaderCell>
            )
          }
        </Table.Row>
      </Table.Header>
      <Table.Body>
        {
          Array.from(iterMap<RankUserInfoModel>(rankInfo.userInfos)).sort((v1, v2) => v1[1].rank > v2[1].rank ? 1 : (v1[1].rank < v2[1].rank ? -1 : 0)).map((v, i) => <Table.Row key={i}>
            <Table.Cell textAlign='center'>{v[1].rank}</Table.Cell>
            <Table.Cell textAlign='center'><NavLink to={`/user/${v[0]}`}>{v[1].userName + (!!v[1].name ? ` (${v[1].name})` : '')}</NavLink></Table.Cell>
            <Table.Cell textAlign='center'>{v[1].score}</Table.Cell>
            <Table.Cell textAlign='center'>{v[1].time} (-{v[1].penalty})</Table.Cell>
            {
              Array.from(iterMap<RankProblemInfoModel>(rankInfo.problemInfos)).map(r =>
                rankInfo.rankInfos[v[0]][r[0]] as RankContestItemModel).map((r, j) =>
                  r ? <Table.Cell textAlign='center' key={j} negative={!r.accepted} positive={r.accepted}>{r.acceptCount}/{r.submissionCount}<br />{r.score}<br />{r.time} (-{r.penalty})</Table.Cell>
                    : <Table.Cell textAlign='center' key={j}>0/0<br />0<br />0:0:0 (-0)</Table.Cell>
                )
            }
          </Table.Row>)
        }
      </Table.Body>
    </Table></div>;

  const placeHolder = <Placeholder>
    <Placeholder.Paragraph>
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
    </Placeholder.Paragraph>
  </Placeholder>;
  return loaded ? rankTable() : placeHolder;
}

export default Rank;