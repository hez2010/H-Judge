import * as React from 'reactn';
import { setTitle } from '../../utils/titleHelper';
import { Button, Pagination, Table, Form, Input, Select, Placeholder, Rating, Confirm, Popup } from 'semantic-ui-react';
import { Post, Delete } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import { GlobalState } from '../../interfaces/globalState';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';
import { getRating } from '../../utils/ratingHelper';

interface ContestProps {
  groupId?: number
}

interface ContestListItemModel {
  id: number,
  name: string,
  hidden: boolean,
  startTime: Date,
  endTime: Date,
  upvote: number,
  downvote: number
}

interface ContestListModel {
  contests: ContestListItemModel[],
  totalCount: number,
  currentTime: Date
}

interface ContestState {
  contestList: ContestListModel,
  statusFilter: number[],
  page: number,
  deleteItem: number,
  loaded: boolean
}

export default class Contest extends React.Component<ContestProps & CommonProps, ContestState, GlobalState> {
  constructor() {
    super();

    this.renderContestList = this.renderContestList.bind(this);
    this.fetchContestList = this.fetchContestList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);
    this.editContest = this.editContest.bind(this);
    this.deleteContest = this.deleteContest.bind(this);

    this.state = {
      contestList: {
        contests: [],
        totalCount: 0,
        currentTime: new Date(Date.now())
      },
      statusFilter: [0, 1, 2],
      page: 0,
      deleteItem: 0,
      loaded: false
    };
  }

  // a cache for converting `Skip query` to `Where query`
  private idRecord = new Map<number, number>();
  // debounce
  private disableNavi = false;
  private userId = this.global.userInfo.userId;

  componentWillUpdate(_nextProps: ContestProps, _nextState: ContestState) {
    if (this.userId !== this.global.userInfo.userId) {
      this.userId = this.global.userInfo.userId;
      if (!this.props.match.params.page) this.fetchContestList(true, 1);
      else this.fetchContestList(true, this.props.match.params.page);
    }
  }

  fetchContestList(requireTotalCount: boolean, page: number) {
    if (!this.props.groupId && page.toString() !== this.props.match.params.page)
      this.props.history.replace(`/contest/${page}`);
    let form = document.querySelector('#filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = this.state.statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.groupId = this.props.groupId;
    if (this.idRecord.has(page)) req.startId = this.idRecord.get(page)! - 1;

    Post('/contest/list', req)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ContestListModel;
        let countBackup = this.state.contestList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;
        result.currentTime = new Date(result.currentTime.toString());
        for (let c in result.contests) {
          result.contests[c].startTime = new Date(result.contests[c].startTime.toString());
          result.contests[c].endTime = new Date(result.contests[c].endTime.toString());
        }

        if (result.contests.length > 0)
          this.idRecord.set(page + 1, result.contests[result.contests.length - 1].id);
        this.setState({
          contestList: result,
          page: page,
          loaded: true
        } as ContestState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '比赛列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    setTitle('比赛');
    if (!this.props.match.params.page) this.fetchContestList(true, 1);
    else this.fetchContestList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {
    if (this.disableNavi) {
      this.disableNavi = false;
      return;
    }
    if (!this.props.groupId) this.props.history.push(`/details/contest/${index}`);
    else this.props.history.push(`/details/contest/${this.props.groupId}/${index}`);
  }

  editContest(id: number) {
    this.disableNavi = true;
    this.props.history.push(`/edit/contest/${id}`);
  }

  deleteContest(id: number) {
    this.setState({ deleteItem: 0 } as ContestState);
    Delete('/contest/edit', { contestId: id })
      .then(tryJson)
      .then(data => {
        let result = data as ErrorModel;
        if (result.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
        }
        else {
          this.global.commonFuncs.openPortal('成功', '删除成功', 'green');
          this.fetchContestList(true, this.state.page);
        }
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '比赛删除失败', 'red');
        console.log(err);
      })
  }

  renderContestList() {
    const getStatus = (startTime: Date, endTime: Date) => {
      if (this.state.contestList.currentTime >= endTime) return 2;
      if (startTime < this.state.contestList.currentTime) return 1;
      return 0;
    };

    const renderRating = (upvote: number, downvote: number) => {
      let rating = getRating(upvote, downvote);
      return <Popup content={rating.toString()} trigger={<Rating icon='star' rating={Math.round(rating)} maxRating={5} disabled={true} />} />;
    }
    
    return <>
      <Table color='black' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>名称</Table.HeaderCell>
            <Table.HeaderCell>状态</Table.HeaderCell>
            <Table.HeaderCell>评分</Table.HeaderCell>
            <Table.HeaderCell>开始时间</Table.HeaderCell>
            <Table.HeaderCell>结束时间</Table.HeaderCell>
            {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.HeaderCell textAlign='center'>操作</Table.HeaderCell> : null}
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.contestList.contests.map((v, i) => {
              let status = getStatus(v.startTime, v.endTime);
              return <Table.Row key={i} warning={v.hidden} className={status === 1 ? 'success' : ''} onClick={() => this.gotoDetails(v.id)} style={{ cursor: 'pointer' }}>
                <Table.Cell>{v.id}</Table.Cell>
                <Table.Cell>{v.name}</Table.Cell>
                <Table.Cell>{status === 0 ? '未开始' : status === 1 ? '进行中' : '已结束'}</Table.Cell>
                <Table.Cell>{renderRating(v.upvote, v.downvote)}</Table.Cell>
                <Table.Cell>{v.startTime.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                <Table.Cell>{v.endTime.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.Cell textAlign='center'><Button.Group><Button onClick={() => this.editContest(v.id)} color='grey'>编辑</Button><Button onClick={() => { this.disableNavi = true; this.setState({ deleteItem: v.id } as ContestState); }} color='red'>删除</Button></Button.Group></Table.Cell> : null}
              </Table.Row>;
            })
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
            <label>比赛编号</label>
            <Input fluid name='id' type='number' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <label>比赛名称</label>
            <Input fluid name='name' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <label>比赛状态</label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as ContestState); this.idRecord.clear(); }} fluid name='status' multiple defaultValue={[0, 1, 2]} options={[{ text: '未开始', value: 0 }, { text: '进行中', value: 1 }, { text: '已结束', value: 2 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <label>比赛操作</label>
            <Button.Group fluid>
              <Button type='button' primary onClick={() => this.fetchContestList(true, 1)}>筛选</Button>
              {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button type='button' secondary onClick={() => this.editContest(0)}>添加</Button> : null}
            </Button.Group>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderContestList()}
      {this.state.loaded ? (this.state.contestList.contests.length === 0 ? <p>没有数据</p> : null) : placeHolder}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.state.page}
          onPageChange={(_event, data) => this.fetchContestList(false, data.activePage as number)}
          size='small'
          siblingRange={2}
          boundaryRange={1}
          totalPages={this.state.contestList.totalCount === 0 ? 0 : Math.floor(this.state.contestList.totalCount / 10) + (this.state.contestList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
      <Confirm
        open={this.state.deleteItem !== 0}
        cancelButton='取消'
        confirmButton='确定'
        onCancel={() => this.setState({ deleteItem: 0 } as ContestState)}
        onConfirm={() => this.deleteContest(this.state.deleteItem)}
        content={"删除后不可恢复，确定继续？"}
      />
    </>;
  }
}