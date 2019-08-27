import * as React from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { Post } from '../../utils/requestHelper';
import { Table, Button, Placeholder, Form, Input, Pagination } from 'semantic-ui-react';
import { tryJson } from '../../utils/responseHelper';
import { NavLink } from 'react-router-dom';

interface StatisticsProps extends CommonProps {
  problemId?: number,
  contestId?: number,
  groupId?: number,
  userId?: string,
  result?: string
}

interface StatisticsItemModel {
  problemId: number,
  contestId: number,
  groupId: number,
  resultId: number,
  problemName: string,
  userId: number,
  userName: number,
  result: string,
  resultType: number,
  time: Date
}

interface StatisticsListModel {
  statistics: StatisticsItemModel[],
  totalCount: number
}

interface StatisticsState {
  statisticsList: StatisticsListModel,
  page: number,
  loaded: boolean
}

const judgeResult = [
  { key: '-2', value: 'All', text: 'All' },
  { key: '-1', value: 'Pending', text: 'Pending' },
  { key: '0', value: 'Judging', text: 'Judging' },
  { key: '1', value: 'Accepted', text: 'Accepted' },
  { key: '2', value: 'Wrong_Answer', text: 'Wrong Answer' },
  { key: '3', value: 'Compile_Error', text: 'Compile Error' },
  { key: '4', value: 'Time_Limit_Exceeded', text: 'Time Limit Exceeded' },
  { key: '5', value: 'Memory_Limit_Exceeded', text: 'Memory Limit Exceeded' },
  { key: '6', value: 'Presentation_Error', text: 'Presentation Error' },
  { key: '7', value: 'Runtime_Error', text: 'Runtime Error' },
  { key: '8', value: 'Special_Judge_Error', text: 'Special Judge Error' },
  { key: '9', value: 'Problem_Config_Error', text: 'Problem Config Error' },
  { key: '10', value: 'Output_File_Error', text: 'Output File Error' },
  { key: '11', value: 'Unknown_Error', text: 'Unknown Error' }
];

export default class Statistics extends React.Component<StatisticsProps, StatisticsState, GlobalState> {
  constructor() {
    super();

    this.renderStatisticsList = this.renderStatisticsList.bind(this);
    this.fetchStatisticsList = this.fetchStatisticsList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);
    this.getNumber = this.getNumber.bind(this);
    this.getString = this.getString.bind(this);

    this.state = {
      statisticsList: {
        statistics: [],
        totalCount: 0
      },
      page: 0,
      loaded: false
    };
  }

  private idRecord = new Map<number, number>();
  private disableNavi = false;
  private expandParamsInUri = false;
  private prevUserId = this.global.userInfo.userId;
  private userId = '';
  private problemId = 0;
  private contestId = -1;
  private groupId = -1;
  private result = '';

  fetchStatisticsList(requireTotalCount: boolean, page: number) {
    if (!this.props.groupId && !this.props.problemId && !this.props.contestId && page.toString() !== this.props.match.params.page) {
      if (this.expandParamsInUri)
        this.props.history.replace(`/statistics/${!!this.userId ? this.userId : '-1'}/${this.groupId}/${this.contestId}/${this.problemId}/${!!this.result ? this.result : 'All'}/${page}`);
      else
        this.props.history.replace(`/statistics/${page}`);
    }
    let req: any = {};
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.problemId = this.problemId === -1 ? null : this.problemId;
    req.contestId = this.contestId === -1 ? null : this.contestId;
    req.groupId = this.groupId === -1 ? null : this.groupId;
    req.userId = !!this.userId ? this.userId : null;
    req.result = this.result;
    if (this.idRecord.has(page)) req.startId = this.idRecord.get(page)! + 1;

    Post('/statistics/list', req)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as StatisticsListModel;
        let countBackup = this.state.statisticsList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;

        result.statistics = result.statistics.map(v => {
          v.time = new Date(v.time);
          return v;
        });

        if (result.statistics.length > 0)
          this.idRecord.set(page + 1, result.statistics[result.statistics.length - 1].resultId);
        this.setState({
          statisticsList: result,
          page: page,
          loaded: true
        } as StatisticsState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '状态列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    if (!this.props.problemId && !this.props.contestId && !this.props.groupId) setTitle('状态');

    if (this.props.groupId) this.groupId = this.props.groupId;
    else if (this.props.match.params.groupId) {
      this.groupId = parseInt(this.props.match.params.groupId);
      this.expandParamsInUri = true;
    }
    if (this.props.contestId) this.contestId = this.props.contestId;
    else if (this.props.match.params.contestId) {
      this.contestId = parseInt(this.props.match.params.contestId);
      this.expandParamsInUri = true;
    }
    if (this.props.problemId) this.problemId = this.props.problemId;
    else if (this.props.match.params.problemId) {
      this.problemId = parseInt(this.props.match.params.problemId);
      this.expandParamsInUri = true;
    }
    if (this.props.userId) this.userId = this.props.userId;
    else if (!!this.props.match.params.userId && this.props.match.params.userId !== '-1') {
      this.userId = this.props.match.params.userId;
      this.expandParamsInUri = true;
    }
    if (this.props.result) this.result = this.props.result;
    else if (!!this.props.match.params.result) {
      this.result = this.props.match.params.result;
      this.expandParamsInUri = true;
    }
    if (!this.props.match.params.page) this.fetchStatisticsList(true, 1);
    else this.fetchStatisticsList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {
    if (this.disableNavi) {
      this.disableNavi = false;
      return;
    }
    this.props.history.push(`/result/${index}`);
  }

  componentWillUpdate(_nextProps: StatisticsProps, _nextState: StatisticsState) {
    if (this.prevUserId !== this.global.userInfo.userId) {
      this.prevUserId = this.global.userInfo.userId;
      this.idRecord.clear();
      if (!this.props.match.params.page) this.fetchStatisticsList(true, 1);
      else this.fetchStatisticsList(true, this.props.match.params.page);
    }
  }

  renderStatisticsList() {
    return <>
      <Table color='black' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>提交编号</Table.HeaderCell>
            <Table.HeaderCell>提交用户</Table.HeaderCell>
            <Table.HeaderCell>题目名称</Table.HeaderCell>
            <Table.HeaderCell>提交时间</Table.HeaderCell>
            <Table.HeaderCell>评测结果</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.statisticsList.statistics.map((v, i) =>
              <Table.Row key={i} onClick={() => this.gotoDetails(v.resultId)} style={{ cursor: 'pointer' }}>
                <Table.Cell>#{v.resultId}</Table.Cell>
                <Table.Cell><NavLink onClick={() => this.disableNavi = true} to={`/user/${v.userId}`}>{v.userName}</NavLink></Table.Cell>
                <Table.Cell><NavLink onClick={() => this.disableNavi = true} to={`/details/problem/${v.problemId}${!v.contestId ? '' : `/${v.contestId}`}${!v.groupId ? '' : `/${v.groupId}`}`}>{v.problemName}</NavLink></Table.Cell>
                <Table.Cell>{v.time.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                <Table.Cell>{v.result}</Table.Cell>
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
  }

  getString(value: number) {
    return value === -1 ? '0' : (value === 0 ? '' : value.toString());
  }

  getNumber(value: string) {
    return value ? (value === '0' ? -1 : parseInt(value)) : 0;
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

    return <>
      <Form id='filterForm'>
        <Form.Group widths={'equal'}>
          <Form.Field width={2}>
            <label>题目编号</label>
            <Input fluid name='problemId' defaultValue={this.state.loaded ? this.getString(this.problemId) : ''} onChange={e => { this.idRecord.clear(); this.problemId = this.getNumber(e.target.value) }}></Input>
          </Form.Field>
          <Form.Field width={2}>
            <label>比赛编号</label>
            <Input fluid name='contestId' defaultValue={this.state.loaded ? this.getString(this.contestId) : ''} onChange={e => { this.idRecord.clear(); this.contestId = this.getNumber(e.target.value) }}></Input>
          </Form.Field>
          <Form.Field width={2}>
            <label>小组编号</label>
            <Input fluid name='groupId' defaultValue={this.state.loaded ? this.getString(this.groupId) : ''} onChange={e => { this.idRecord.clear(); this.groupId = this.getNumber(e.target.value) }}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <label>用户编号</label>
            <Input fluid name='userId' defaultValue={this.state.loaded ? this.userId : ''} onChange={e => { this.idRecord.clear(); this.userId = e.target.value; }}></Input>
          </Form.Field>
          <Form.Field width={4}>
            <label>评测结果</label>
            <Form.Select
              fluid
              defaultValue={this.state.loaded ? this.result : 'All'}
              options={judgeResult}
              onChange={(_, data) => { this.idRecord.clear(); this.result = data.value as string; }}
            />
          </Form.Field>
          <Form.Field width={2}>
            <label>题目操作</label>
            <Button.Group fluid>
              <Button type='button' primary onClick={() => { this.props.match.params.page = 0; this.expandParamsInUri = true; this.fetchStatisticsList(true, 1); }}>筛选</Button>
            </Button.Group>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderStatisticsList()}
      {this.state.loaded ? (this.state.statisticsList.statistics.length === 0 ? <p>没有数据</p> : null) : placeHolder}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.state.page}
          onPageChange={(_event, data) => this.fetchStatisticsList(false, data.activePage as number)}
          size='small'
          siblingRange={2}
          boundaryRange={1}
          totalPages={this.state.statisticsList.totalCount === 0 ? 0 : Math.floor(this.state.statisticsList.totalCount / 10) + (this.state.statisticsList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
    </>;
  }
}