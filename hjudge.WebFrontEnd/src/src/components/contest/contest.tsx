import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Button, Pagination, Table, Form, Label, Input, Select, Placeholder, Rating } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';

interface ContestProps extends CommonProps {
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

interface ContestListModel extends ResultModel {
  contests: ContestListItemModel[],
  totalCount: number,
  currentTime: Date
}

interface ContestState {
  contestList: ContestListModel,
  statusFilter: number[]
}

export default class Contest extends React.Component<ContestProps, ContestState> {
  constructor(props: ContestProps) {
    super(props);

    this.renderContestList = this.renderContestList.bind(this);
    this.fetchContestList = this.fetchContestList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);
    this.editContest = this.editContest.bind(this);

    this.state = {
      contestList: {
        contests: [],
        totalCount: 0,
        currentTime: new Date(Date.now())
      },
      statusFilter: [0, 1, 2]
    };

    this.idRecord = new Map<number, number>();
  }
  private idRecord: Map<number, number>;

  componentWillUpdate(nextProps: any, nextState: any) {
    if (nextProps.userInfo.userId !== this.props.userInfo.userId) {
      this.idRecord.clear();
      if (!this.props.match.params.page) this.fetchContestList(true, 1);
      else this.fetchContestList(true, this.props.match.params.page);
    }
  }

  fetchContestList(requireTotalCount: boolean, page: number) {
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

    Post('/Contest/ContestList', req)
      .then(res => res.json())
      .then(data => {
        let result = data as ContestListModel;
        if (result.succeeded) {
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
            contestList: result
          } as ContestState);
        }
        else {
          this.props.openPortal('错误', `比赛列表加载失败\n${result.errorMessage} (${result.errorCode})`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '比赛列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    setTitle('比赛');
    if (!this.props.match.params.page) this.fetchContestList(true, 1);
    else this.fetchContestList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {
    if (!this.props.groupId) this.props.history.push(`/details/contest/${index}`);
    else this.props.history.push(`/details/contest/${this.props.groupId}/${index}`);
  }

  editContest(id: number) {
    this.props.history.push(`/edit/contest/${id}`);
  }

  renderContestList() {
    let getStatus = (startTime: Date, endTime: Date) => {
      if (startTime < this.state.contestList.currentTime) return 0;
      if (this.state.contestList.currentTime > endTime) return 2;
      return 1;
    };

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
            {this.props.userInfo.succeeded && isTeacher(this.props.userInfo.privilege) ? <Table.HeaderCell textAlign='center'>操作</Table.HeaderCell> : null}
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
                {
                  v.upvote + v.downvote === 0 ?
                    <Table.Cell><Rating icon='star' defaultRating={3} maxRating={5} disabled={true} /></Table.Cell> :
                    <Table.Cell><Rating icon='star' defaultRating={3} maxRating={5} disabled={true} rating={Math.round(v.upvote * 5 / (v.upvote + v.downvote))} /></Table.Cell>
                }
                <Table.Cell>{v.startTime.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                <Table.Cell>{v.endTime.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                {this.props.userInfo.succeeded && isTeacher(this.props.userInfo.privilege) ? <Table.Cell textAlign='center'><Button.Group><Button color='grey'>编辑</Button><Button color='red'>删除</Button></Button.Group></Table.Cell> : null}
              </Table.Row>;
            })
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
          <Form.Field width={6}>
            <Label>比赛编号</Label>
            <Input fluid name='id' type='number' onChange={() => { this.idRecord.clear(); }}></Input>
          </Form.Field>
          <Form.Field>
            <Label>比赛名称</Label>
            <Input fluid name='name' onChange={() => { this.idRecord.clear(); }}></Input>
          </Form.Field>
          <Form.Field>
            <Label>比赛状态</Label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as ContestState); this.idRecord.clear(); }} fluid name='status' multiple defaultValue={[0, 1, 2]} options={[{ text: '未开始', value: 0 }, { text: '进行中', value: 1 }, { text: '已结束', value: 2 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <Label>比赛操作</Label>
            <Button.Group fluid>
              <Button primary onClick={() => this.fetchContestList(true, 1)}>筛选</Button>
              {this.props.userInfo.succeeded && isTeacher(this.props.userInfo.privilege) ? <Button secondary onClick={() => this.editContest(0)}>添加</Button> : null}
            </Button.Group>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderContestList()}
      {this.state.contestList.succeeded ? (this.state.contestList.contests.length === 0 ? <p>没有数据</p> : null) : placeHolder}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.state.contestList.totalCount === 0 ? 0 : this.props.match.params.page}
          onPageChange={(_event, data) => this.fetchContestList(false, data.activePage as number)}
          size='small'
          siblingRange={3}
          boundaryRange={1}
          totalPages={this.state.contestList.totalCount === 0 ? 0 : Math.floor(this.state.contestList.totalCount / 10) + (this.state.contestList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
    </>;
  }
}