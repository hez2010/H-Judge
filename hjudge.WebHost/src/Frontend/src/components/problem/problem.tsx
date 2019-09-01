import * as React from 'reactn';
import { setTitle } from '../../utils/titleHelper';
import { Button, Pagination, Table, Form, Input, Select, Placeholder, Rating, Confirm } from 'semantic-ui-react';
import { Post, Delete } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import { GlobalState } from '../../interfaces/globalState';
import { tryJson } from '../../utils/responseHelper';

interface ProblemProps {
  contestId?: number,
  groupId?: number
}

export interface ProblemListItemModel {
  id: number,
  name: string,
  level: number,
  hidden: boolean,
  status: number,
  acceptCount: number,
  submissionCount: number,
  upvote: number,
  downvote: number
}

interface ProblemListModel {
  problems: ProblemListItemModel[],
  totalCount: number
}

interface ProblemState {
  problemList: ProblemListModel,
  statusFilter: number[],
  deleteItem: number,
  page: number,
  loaded: boolean
}

export default class Problem extends React.Component<ProblemProps & CommonProps, ProblemState, GlobalState> {
  constructor() {
    super();
    this.renderProblemList = this.renderProblemList.bind(this);
    this.fetchProblemList = this.fetchProblemList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);
    this.editProblem = this.editProblem.bind(this);
    this.deleteProblem = this.deleteProblem.bind(this);

    this.state = {
      problemList: {
        problems: [],
        totalCount: 0
      },
      statusFilter: [0, 1, 2],
      deleteItem: 0,
      page: 0,
      loaded: false
    };
  }

  private idRecord = new Map<number, number>();
  private disableNavi = false;
  private userId = this.global.userInfo.userId;

  componentWillUpdate(_nextProps: ProblemProps, _nextState: ProblemState) {
    if (this.userId !== this.global.userInfo.userId) {
      this.userId = this.global.userInfo.userId;
      this.idRecord.clear();
      if (!this.props.match.params.page) this.fetchProblemList(true, 1);
      else this.fetchProblemList(true, this.props.match.params.page);
    }
  }

  fetchProblemList(requireTotalCount: boolean, page: number) {
    if (!this.props.contestId && page.toString() !== this.props.match.params.page)
      this.props.history.replace(`/problem/${page}`);
    let form = document.querySelector('#filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = this.state.statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.contestId = this.props.contestId;
    req.groupId = this.props.groupId;
    if (this.idRecord.has(page)) req.startId = this.idRecord.get(page)! + 1;

    Post('/problem/list', req)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemListModel;
        let countBackup = this.state.problemList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;

        if (result.problems.length > 0)
          this.idRecord.set(page + 1, result.problems[result.problems.length - 1].id);
        this.setState({
          problemList: result,
          page: page,
          loaded: true
        } as ProblemState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    if (!this.props.contestId && !this.props.groupId) setTitle('题库');

    if (!this.props.match.params.page) this.fetchProblemList(true, 1);
    else this.fetchProblemList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {
    if (this.disableNavi) {
      this.disableNavi = false;
      return;
    }
    if (!this.props.contestId) this.props.history.push(`/details/problem/${index}`);
    else if (!this.props.groupId) this.props.history.push(`/details/problem/${index}/${this.props.contestId}`);
    else this.props.history.push(`/details/problem/${index}/${this.props.contestId}/${this.props.groupId}`);
  }

  editProblem(id: number) {
    this.disableNavi = true;
    this.props.history.push(`/edit/problem/${id}`);
  }

  deleteProblem(id: number) {
    this.setState({ deleteItem: 0 } as ProblemState);
    Delete('/problem/edit', { problemId: id })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('成功', '题目删除成功', 'green');
        this.fetchProblemList(true, this.state.page);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目删除失败', 'red');
        console.log(err);
      })
  }

  renderProblemList() {
    return <>
      <Table color='black' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>名称</Table.HeaderCell>
            <Table.HeaderCell>难度</Table.HeaderCell>
            <Table.HeaderCell>状态</Table.HeaderCell>
            <Table.HeaderCell>评分</Table.HeaderCell>
            <Table.HeaderCell>通过量/提交量</Table.HeaderCell>
            <Table.HeaderCell>通过率</Table.HeaderCell>
            {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.HeaderCell textAlign='center'>操作</Table.HeaderCell> : null}
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.problemList.problems.map((v, i) =>
              <Table.Row key={i} warning={v.hidden} onClick={() => this.gotoDetails(v.id)} style={{ cursor: 'pointer' }}>
                <Table.Cell>{v.id}</Table.Cell>
                <Table.Cell>{v.name}</Table.Cell>
                <Table.Cell><span role='img' aria-label='level'>⭐</span>×{v.level}</Table.Cell>
                <Table.Cell>{v.status === 0 ? '未尝试' : v.status === 1 ? '已尝试' : '已通过'}</Table.Cell>
                {
                  v.upvote + v.downvote === 0 ?
                    <Table.Cell><Rating icon='star' defaultRating={3} maxRating={5} disabled={true} /></Table.Cell> :
                    <Table.Cell><Rating icon='star' maxRating={5} disabled={true} rating={Math.round(v.upvote * 5 / (v.upvote + v.downvote))} /></Table.Cell>
                }
                <Table.Cell>{v.acceptCount}/{v.submissionCount}</Table.Cell>
                <Table.Cell>{v.submissionCount === 0 ? 0 : Math.round(v.acceptCount * 10000 / v.submissionCount) / 100.0} %</Table.Cell>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.Cell textAlign='center'><Button.Group><Button onClick={() => this.editProblem(v.id)} color='grey'>编辑</Button><Button onClick={() => { this.disableNavi = true; this.setState({ deleteItem: v.id } as ProblemState); }} color='red'>删除</Button></Button.Group></Table.Cell> : null}
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
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
          <Form.Field width={4}>
            <label>题目编号</label>
            <Input fluid name='id' type='number' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <label>题目名称</label>
            <Input fluid name='name' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <label>题目状态</label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as ProblemState); this.idRecord.clear(); }} fluid name='status' multiple defaultValue={[0, 1, 2]} options={[{ text: '未尝试', value: 0 }, { text: '已尝试', value: 1 }, { text: '已通过', value: 2 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <label>题目操作</label>
            <Button.Group fluid>
              <Button type='button' primary onClick={() => this.fetchProblemList(true, 1)}>筛选</Button>
              {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button type='button' secondary onClick={() => this.editProblem(0)}>添加</Button> : null}
            </Button.Group>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderProblemList()}
      {this.state.loaded ? (this.state.problemList.problems.length === 0 ? <p>没有数据</p> : null) : placeHolder}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.state.page}
          onPageChange={(_event, data) => this.fetchProblemList(false, data.activePage as number)}
          size='small'
          siblingRange={2}
          boundaryRange={1}
          totalPages={this.state.problemList.totalCount === 0 ? 0 : Math.floor(this.state.problemList.totalCount / 10) + (this.state.problemList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
      <Confirm
        open={this.state.deleteItem !== 0}
        cancelButton='取消'
        confirmButton='确定'
        onCancel={() => this.setState({ deleteItem: 0 } as ProblemState)}
        onConfirm={() => this.deleteProblem(this.state.deleteItem)}
        content={"删除后不可恢复，确定继续？"}
      />
    </>;
  }
}