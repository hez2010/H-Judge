import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { match } from 'react-router';
import { Button, Pagination, Table, Form, Label, Input, Select, SemanticCOLORS } from 'semantic-ui-react';
import { History, Location } from 'history';
import { Post } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { ResultModel } from '../../interfaces/resultModel';

interface ProblemProps {
  match: match<any>,
  history: History<any>,
  location: Location<any>,
  openPortal: ((header: string, message: string, color: SemanticCOLORS) => void),
  contestId?: number,
  groupId?: number
}

interface ProblemListItemModel {
  id: number,
  name: string,
  level: number,
  hidden: boolean,
  status: number,
  acceptCount: number,
  submissionCount: number
}

interface ProblemListModel extends ResultModel {
  problems: ProblemListItemModel[],
  totalCount: number
}

interface ProblemState {
  problemList: ProblemListModel,
  statusFilter: number[]
}

export default class Problem extends React.Component<ProblemProps, ProblemState> {
  constructor(props: ProblemProps) {
    super(props);

    this.renderProblemList = this.renderProblemList.bind(this);
    this.fetchProblemList = this.fetchProblemList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);

    this.state = {
      problemList: {
        problems: [],
        totalCount: 0
      },
      statusFilter: [0, 1, 2]
    };
  }

  fetchProblemList(requireTotalCount: boolean, page: number) {
    if (!this.props.contestId && page.toString() !== this.props.match.params.page)
      this.props.history.replace(`/problem/${page}`);
    let form = document.getElementById('filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = this.state.statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.contestId = this.props.contestId;
    req.groupId = this.props.groupId;

    Post('/Problem/ProblemList', req)
      .then(res => res.json())
      .then(data => {
        let result = data as ProblemListModel;
        if (result.succeeded) {
          let countBackup = this.state.problemList.totalCount;
          if (!requireTotalCount) result.totalCount = countBackup;
          this.setState({
            problemList: result
          } as ProblemState);
        }
        else {
          this.props.openPortal('错误', `题目列表加载失败\n${result.errorMessage} (${result.errorCode})`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '题目列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    if (!this.props.contestId && !this.props.groupId) setTitle('题库');

    if (!this.props.match.params.page) this.fetchProblemList(true, 1);
    else this.fetchProblemList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {

  }

  renderProblemList() {
    return <>
      <Table color='blue' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>名称</Table.HeaderCell>
            <Table.HeaderCell>难度</Table.HeaderCell>
            <Table.HeaderCell>状态</Table.HeaderCell>
            <Table.HeaderCell>通过量/提交量</Table.HeaderCell>
            <Table.HeaderCell>通过率</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.problemList.problems.map((v, i) =>
              <Table.Row key={i} warning={v.hidden} onClick={() => this.gotoDetails(i)}>
                <Table.Cell>{v.id}</Table.Cell>
                <Table.Cell>{v.name}</Table.Cell>
                <Table.Cell>{v.level}</Table.Cell>
                <Table.Cell>{v.status === 0 ? '未尝试' : v.status === 1 ? '已尝试' : '已通过'}</Table.Cell>
                <Table.Cell>{v.acceptCount}/{v.submissionCount}</Table.Cell>
                <Table.Cell>{v.submissionCount === 0 ? 0 : Math.round(v.acceptCount * 10000 / v.submissionCount) / 100.0} %</Table.Cell>
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
  }

  render() {
    return <>
      <Form id='filterForm'>
        <Form.Group widths={'equal'}>
          <Form.Field width={6}>
            <Label>题目编号</Label>
            <Input fluid name='id' type='number'></Input>
          </Form.Field>
          <Form.Field>
            <Label>题目名称</Label>
            <Input fluid name='name'></Input>
          </Form.Field>
          <Form.Field>
            <Label>题目状态</Label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as ProblemState) }} fluid name='status' multiple defaultValue={[0, 1, 2]} options={[{ text: '未尝试', value: 0 }, { text: '已尝试', value: 1 }, { text: '已通过', value: 2 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <Label>筛选操作</Label>
            <Button fluid primary onClick={() => this.fetchProblemList(true, 1)}>确定</Button>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderProblemList()}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.props.match.params.page}
          onPageChange={(_event, data) => this.fetchProblemList(false, data.activePage as number)}
          size='small'
          siblingRange={3}
          boundaryRange={1}
          totalPages={Math.floor(this.state.problemList.totalCount / 10) + (this.state.problemList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
    </>;
  }
}