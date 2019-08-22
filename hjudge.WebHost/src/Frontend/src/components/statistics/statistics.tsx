import * as React from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { Post } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { Table, Button, Placeholder, Form, Label, Input, Select, Pagination } from 'semantic-ui-react';
import { tryJson } from '../../utils/responseHelper';

interface StatisticsProps extends CommonProps {
  problemId?: number, // unused
  contestId?: number, // unused
  groupId?: number // unused
}

interface StatisticsItemModel {
  problemId: number,
  resultId: number,
  problemName: number,
  userId: number,
  userName: number,
  resultType: string,
  time: Date
}

interface StatisticsListModel {
  statistics: StatisticsItemModel[],
  totalCount: number
}

interface StatisticsState {
  statisticsList: StatisticsListModel,
  statusFilter: number[],
  page: number,
  loaded: boolean
}

export default class Statistics extends React.Component<StatisticsProps, StatisticsState, GlobalState> {
  constructor() {
    super();

    this.renderStatisticsList = this.renderStatisticsList.bind(this);
    this.fetchStatisticsList = this.fetchStatisticsList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);

    this.state = {
      statisticsList: {
        statistics: [],
        totalCount: 0
      },
      statusFilter: [0, 1, 2],
      page: 0,
      loaded: false
    };
  }

  private idRecord = new Map<number, number>();
  private disableNavi = false;
  private userId = this.global.userInfo.userId;

  fetchStatisticsList(requireTotalCount: boolean, page: number) {
    if (!this.props.groupId && !this.props.problemId && !this.props.contestId && page.toString() !== this.props.match.params.page)
      this.props.history.replace(`/statistics/${page}`);
    let form = document.querySelector('#filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = this.state.statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.problemId = this.props.problemId;
    req.contestId = this.props.contestId;
    req.groupId = this.props.groupId;
    if (this.idRecord.has(page)) req.startId = this.idRecord.get(page)! + 1;

    Post('/statistics/list', req)
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as StatisticsListModel;
        let countBackup = this.state.statisticsList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;

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
    if (this.userId !== this.global.userInfo.userId) {
      this.userId = this.global.userInfo.userId;
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
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>用户</Table.HeaderCell>
            <Table.HeaderCell>题目</Table.HeaderCell>
            <Table.HeaderCell>结果</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.statisticsList.statistics.map((v, i) =>
              <Table.Row key={i} onClick={() => this.gotoDetails(v.resultId)} style={{ cursor: 'pointer' }}>
                <Table.Cell>{v.resultId}</Table.Cell>
                <Table.Cell>{v.userName}</Table.Cell>
                <Table.Cell>{v.problemName}</Table.Cell>
                <Table.Cell>{v.resultType}</Table.Cell>
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
  }

  render() {
    let placeHolder = <Placeholder>
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
          <Form.Field width={4}>
            <Label>提交编号</Label>
            <Input fluid name='id' type='number' onChange={this.idRecord.clear}></Input>
          </Form.Field>
          <Form.Field width={4}>
            <Label>题目名称</Label>
            <Input fluid name='problemName' onChange={this.idRecord.clear}></Input>
          </Form.Field>
          <Form.Field width={4}>
            <Label>比赛名称</Label>
            <Input fluid name='contestName' onChange={this.idRecord.clear}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <Label>提交来源</Label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as StatisticsState); this.idRecord.clear(); }} fluid name='status' multiple defaultValue={[0, 1]} options={[{ text: '我的提交', value: 0 }, { text: '全部提交', value: 1 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <Label>题目操作</Label>
            <Button.Group fluid>
              <Button type='button' primary onClick={() => this.fetchStatisticsList(true, 1)}>筛选</Button>
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